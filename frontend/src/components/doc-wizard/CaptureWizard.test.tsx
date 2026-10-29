import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { toast } from 'sonner'
import { describe, expect, it, vi } from 'vitest'
import { CaptureWizard } from './CaptureWizard'
import { WIZARD_STEPS } from './config'
import type { Draft, QualityFlag } from './types'

vi.mock('./useCamera', () => ({
  useCamera: () => ({ status: 'unsupported', stream: null, torchSupported: false, torchOn: false, toggleTorch: vi.fn() }),
}))
// A plain function (not a spy) so a rejected photo does not surface as an unhandled rejection.
const image = vi.hoisted(() => ({ fileToPhoto: (): Promise<unknown> => Promise.resolve(undefined) }))
vi.mock('@/lib/image', () => ({ fileToPhoto: () => image.fileToPhoto(), frameToPhoto: vi.fn() }))
vi.mock('sonner', () => ({ toast: { error: vi.fn() } }))

const draft: Draft = {
  student: {
    id: 's1',
    createdAt: 0,
    slots: WIZARD_STEPS.map((step) => ({ docType: step.id, status: 'empty', qualityFlags: [] })),
  },
  stepIndex: 0,
  mode: 'capture',
  returnTo: null,
}

function setup() {
  const props = { onCapture: vi.fn(), onSkip: vi.fn(), onGoTo: vi.fn(), onAdvance: vi.fn(), onExit: vi.fn() }
  const { container } = render(<CaptureWizard draft={draft} studentNumber={1} {...props} />)
  const input = container.querySelector('input[type="file"]') as HTMLInputElement
  return { ...props, input }
}

const photo = (flags: QualityFlag[]) => ({ blob: new Blob(['x']), flags })
const upload = (input: HTMLInputElement) =>
  userEvent.upload(input, new File(['x'], 'doc.jpg', { type: 'image/jpeg' }))

describe('CaptureWizard', () => {
    it('shows the hint of the current step', () => {
    setup()
    expect(screen.getByText(WIZARD_STEPS[0].hintText)).toBeInTheDocument()
  })

  it('skips the current document and advances', async () => {
    const { onSkip, onAdvance } = setup()
    await userEvent.click(screen.getByRole('button', { name: 'No disponible' }))
    expect(onSkip).toHaveBeenCalledWith(0)
    expect(onAdvance).toHaveBeenCalledWith(0)
  })

  it('records a good photo from the file fallback and advances', async () => {
    image.fileToPhoto = () => Promise.resolve(photo([]))
    const { onCapture, onAdvance, input } = setup()
    await upload(input)
    await waitFor(() => expect(onCapture).toHaveBeenCalledWith(0, expect.any(Blob), []))
    await waitFor(() => expect(onAdvance).toHaveBeenCalledWith(0))
  })

  it('warns about a bad photo and waits for the user instead of advancing', async () => {
    image.fileToPhoto = () => Promise.resolve(photo(['blur']))
    const { onCapture, onAdvance, input } = setup()
    await upload(input)
    expect(await screen.findByRole('alert')).toHaveTextContent('La foto se ve borrosa.')
    expect(onCapture).toHaveBeenCalledWith(0, expect.any(Blob), ['blur'])
    expect(onAdvance).not.toHaveBeenCalled()
  })

  it('advances when the user continues despite the warning', async () => {
    image.fileToPhoto = () => Promise.resolve(photo(['dark']))
    const { onAdvance, input } = setup()
    await upload(input)
    await userEvent.click(await screen.findByRole('button', { name: 'Continuar' }))
    expect(onAdvance).toHaveBeenCalledWith(0)
  })

  it('dismisses the warning when the user retakes', async () => {
    image.fileToPhoto = () => Promise.resolve(photo(['dark']))
    const { onAdvance, input } = setup()
    await upload(input)
    await userEvent.click(await screen.findByRole('button', { name: 'Repetir' }))
    await waitFor(() => expect(screen.queryByRole('alert')).not.toBeInTheDocument())
    expect(onAdvance).not.toHaveBeenCalled()
  })

  it('reports a processing failure without recording a photo', async () => {
    image.fileToPhoto = () => Promise.reject(new Error('bad image'))
    const { onCapture, input } = setup()
    await upload(input)
    await waitFor(() => expect(toast.error).toHaveBeenCalled())
    expect(onCapture).not.toHaveBeenCalled()
  })
})

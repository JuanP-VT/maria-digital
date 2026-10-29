import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { WIZARD_STEPS } from './config'
import { PreviewScreen } from './PreviewScreen'
import type { Slot } from './types'

function slotsWith(statuses: Slot['status'][]): Slot[] {
  return WIZARD_STEPS.map((step, index) => ({
    docType: step.id,
    status: statuses[index] ?? 'empty',
    image: statuses[index] === 'captured' ? new Blob(['x'], { type: 'image/jpeg' }) : undefined,
    qualityFlags: [],
  }))
}

function setup(statuses: Slot['status'][]) {
  const props = { onRetake: vi.fn(), onSave: vi.fn(), onExit: vi.fn() }
  render(<PreviewScreen slots={slotsWith(statuses)} studentNumber={3} saving={false} {...props} />)
  return props
}

const ALL_REQUIRED: Slot['status'][] = ['captured', 'captured', 'captured']

describe('PreviewScreen', () => {
  it('shows the captured count for the student', () => {
    setup(ALL_REQUIRED)
    expect(screen.getByText(`Estudiante 3 · 3 de ${WIZARD_STEPS.length} capturados`)).toBeInTheDocument()
  })

  it('saves directly when every required document is captured', async () => {
    const { onSave } = setup(ALL_REQUIRED)
    await userEvent.click(screen.getByRole('button', { name: 'Guardar estudiante' }))
    expect(onSave).toHaveBeenCalledOnce()
  })

  it('asks for confirmation when a required document is missing, naming it', async () => {
    const { onSave } = setup(['captured', 'skipped', 'captured'])
    await userEvent.click(screen.getByRole('button', { name: 'Guardar estudiante' }))
    expect(onSave).not.toHaveBeenCalled()
    expect(await screen.findByText(/Falta: Acta de nacimiento\./)).toBeInTheDocument()
  })

  it('saves after confirming despite missing documents', async () => {
    const { onSave } = setup(['captured', 'empty', 'captured'])
    await userEvent.click(screen.getByRole('button', { name: 'Guardar estudiante' }))
    await userEvent.click(await screen.findByRole('button', { name: 'Guardar de todas formas' }))
    expect(onSave).toHaveBeenCalledOnce()
  })

  it('does not save when the user keeps reviewing', async () => {
    const { onSave } = setup(['empty', 'captured', 'captured'])
    await userEvent.click(screen.getByRole('button', { name: 'Guardar estudiante' }))
    await userEvent.click(await screen.findByRole('button', { name: 'Seguir revisando' }))
    expect(onSave).not.toHaveBeenCalled()
  })

  it('retakes the document of the chosen slot', async () => {
    const { onRetake } = setup(['captured', 'empty', 'captured'])
    await userEvent.click(screen.getAllByRole('button', { name: /Capturar|Repetir/ })[1])
    expect(onRetake).toHaveBeenCalledWith(1)
  })

  it('does not require the optional document', async () => {
    const { onSave } = setup([...ALL_REQUIRED, 'empty'])
    await userEvent.click(screen.getByRole('button', { name: 'Guardar estudiante' }))
    expect(onSave).toHaveBeenCalledOnce()
  })
})

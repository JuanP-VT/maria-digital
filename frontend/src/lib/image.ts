import type { QualityFlag } from '@/components/doc-wizard/types'

// Output size/quality of stored photos.
const MAX_EDGE = 1800
const JPEG_QUALITY = 0.8

// Quality heuristics (the backend does the real judging). Tune from real device shots.
const ANALYSIS_EDGE = 400
const DARK_MEAN_LUMA = 70
const BLUR_LAPLACIAN_VARIANCE = 30

export type ProcessedPhoto = { blob: Blob; flags: QualityFlag[] }

type Source = { image: CanvasImageSource; width: number; height: number }

function drawScaled({ image, width, height }: Source, maxEdge: number) {
  const scale = Math.min(1, maxEdge / Math.max(width, height))
  const canvas = document.createElement('canvas')
  canvas.width = Math.max(1, Math.round(width * scale))
  canvas.height = Math.max(1, Math.round(height * scale))
  canvas.getContext('2d')!.drawImage(image, 0, 0, canvas.width, canvas.height)
  return canvas
}

function analyze(canvas: HTMLCanvasElement): QualityFlag[] {
  const small = drawScaled({ image: canvas, width: canvas.width, height: canvas.height }, ANALYSIS_EDGE)
  const { width, height } = small
  const { data } = small.getContext('2d', { willReadFrequently: true })!.getImageData(0, 0, width, height)

  const gray = new Float32Array(width * height)
  let sum = 0
  for (let i = 0; i < gray.length; i++) {
    const value = 0.299 * data[i * 4] + 0.587 * data[i * 4 + 1] + 0.114 * data[i * 4 + 2]
    gray[i] = value
    sum += value
  }

  let lapSum = 0
  let lapSumSq = 0
  let count = 0
  for (let y = 1; y < height - 1; y++) {
    for (let x = 1; x < width - 1; x++) {
      const i = y * width + x
      const lap = gray[i - 1] + gray[i + 1] + gray[i - width] + gray[i + width] - 4 * gray[i]
      lapSum += lap
      lapSumSq += lap * lap
      count++
    }
  }
  const lapMean = lapSum / count
  const variance = lapSumSq / count - lapMean * lapMean

  const flags: QualityFlag[] = []
  if (sum / gray.length < DARK_MEAN_LUMA) flags.push('dark')
  if (variance < BLUR_LAPLACIAN_VARIANCE) flags.push('blur')
  return flags
}

function toBlob(canvas: HTMLCanvasElement) {
  return new Promise<Blob>((resolve, reject) => {
    canvas.toBlob((blob) => (blob ? resolve(blob) : reject(new Error('No se pudo procesar la foto.'))), 'image/jpeg', JPEG_QUALITY)
  })
}

async function process(source: Source): Promise<ProcessedPhoto> {
  const canvas = drawScaled(source, MAX_EDGE)
  const flags = analyze(canvas)
  return { blob: await toBlob(canvas), flags }
}

export function frameToPhoto(video: HTMLVideoElement) {
  return process({ image: video, width: video.videoWidth, height: video.videoHeight })
}

async function decodeFile(file: Blob): Promise<Source> {
  try {
    const bitmap = await createImageBitmap(file, { imageOrientation: 'from-image' })
    return { image: bitmap, width: bitmap.width, height: bitmap.height }
  } catch {
    const url = URL.createObjectURL(file)
    try {
      const image = new Image()
      image.src = url
      await image.decode()
      return { image, width: image.naturalWidth, height: image.naturalHeight }
    } finally {
      URL.revokeObjectURL(url)
    }
  }
}

export async function fileToPhoto(file: Blob) {
  return process(await decodeFile(file))
}

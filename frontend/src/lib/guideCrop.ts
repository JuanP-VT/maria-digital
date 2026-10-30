// The on-screen document guide (see Viewfinder). Keep these in sync with its CSS:
// height is 68% of the viewfinder, width follows the A4 ratio and never exceeds 88%.
export const GUIDE_HEIGHT_RATIO = 0.68
export const GUIDE_MAX_WIDTH_RATIO = 0.88
export const GUIDE_ASPECT = 1 / 1.414

export type Size = { width: number; height: number }
export type CropRect = { x: number; y: number; width: number; height: number }

/** The guide rectangle in viewfinder pixels, centered. */
export function guideRect(box: Size): CropRect {
  const height = Math.min(box.height * GUIDE_HEIGHT_RATIO, (box.width * GUIDE_MAX_WIDTH_RATIO) / GUIDE_ASPECT)
  const width = height * GUIDE_ASPECT
  return { x: (box.width - width) / 2, y: (box.height - height) / 2, width, height }
}

/**
 * Maps the on-screen guide back to video pixels. The video is displayed with `object-cover`,
 * so it is scaled to fill the box and the overflow is cut off evenly on both sides.
 */
export function guideCropInVideo(video: Size, box: Size): CropRect {
  const scale = Math.max(box.width / video.width, box.height / video.height)
  const offsetX = (video.width * scale - box.width) / 2
  const offsetY = (video.height * scale - box.height) / 2
  const guide = guideRect(box)
  return {
    x: Math.max(0, (guide.x + offsetX) / scale),
    y: Math.max(0, (guide.y + offsetY) / scale),
    width: Math.min(video.width, guide.width / scale),
    height: Math.min(video.height, guide.height / scale),
  }
}

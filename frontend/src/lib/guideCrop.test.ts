import { describe, expect, it } from 'vitest'
import { guideCropInVideo, guideRect } from './guideCrop'

describe('guideRect', () => {
  it('is centered, 68% tall and A4-shaped', () => {
    const rect = guideRect({ width: 400, height: 700 })
    expect(rect.height).toBeCloseTo(476)
    expect(rect.width / rect.height).toBeCloseTo(1 / 1.414)
    expect(rect.x).toBeCloseTo((400 - rect.width) / 2)
    expect(rect.y).toBeCloseTo((700 - rect.height) / 2)
  })

  it('shrinks to keep the A4 ratio when the box is too narrow', () => {
    const rect = guideRect({ width: 200, height: 700 })
    expect(rect.width).toBeCloseTo(176)
    expect(rect.width / rect.height).toBeCloseTo(1 / 1.414)
  })
})

describe('guideCropInVideo', () => {
  it('crops a smaller region than the full frame', () => {
    const crop = guideCropInVideo({ width: 1080, height: 1920 }, { width: 400, height: 700 })
    expect(crop.width).toBeLessThan(1080)
    expect(crop.height).toBeLessThan(1920)
    expect(crop.width / crop.height).toBeCloseTo(1 / 1.414)
  })

  it('accounts for the sides cut off by object-cover on a wide video', () => {
    const video = { width: 1600, height: 900 }
    const box = { width: 400, height: 700 }
    const crop = guideCropInVideo(video, box)
    // Scale is 700/900; the visible video window is centered, so the crop is centered too.
    expect(crop.x + crop.width / 2).toBeCloseTo(video.width / 2)
    expect(crop.y + crop.height / 2).toBeCloseTo(video.height / 2)
    expect(crop.height).toBeCloseTo((700 * 0.68) / (700 / 900))
  })

  it('stays inside the video bounds', () => {
    const crop = guideCropInVideo({ width: 640, height: 480 }, { width: 300, height: 300 })
    expect(crop.x).toBeGreaterThanOrEqual(0)
    expect(crop.y).toBeGreaterThanOrEqual(0)
    expect(crop.x + crop.width).toBeLessThanOrEqual(640)
    expect(crop.y + crop.height).toBeLessThanOrEqual(480)
  })
})

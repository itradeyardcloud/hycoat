import { SQ_MM_TO_SQ_FT } from './constants';

/**
 * Calculate area in SFT from perimeter (mm), length (mm), and quantity.
 * Formula: (Perimeter × Length × Qty) / 92903.04
 */
export function calculateAreaSFT(perimeterMM, lengthMM, quantity) {
  if (!perimeterMM || !lengthMM || !quantity) return 0;
  return (perimeterMM * lengthMM * quantity) / SQ_MM_TO_SQ_FT;
}

/**
 * Calculate line amount = areaSFT × ratePerSFT
 */
export function calculateLineAmount(areaSFT, ratePerSFT) {
  if (!areaSFT || !ratePerSFT) return 0;
  return areaSFT * ratePerSFT;
}

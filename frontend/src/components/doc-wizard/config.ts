export type WizardStep = {
  id: string
  label: string
  hintText: string
  required: boolean
  exampleImage?: string
}

// Order of the wizard steps follows the order of this array.
export const WIZARD_STEPS: WizardStep[] = [
  {
    id: 'registration',
    label: 'Hoja de inscripción',
    hintText: 'Hoja de inscripción: colócala plana, con las cuatro esquinas visibles.',
    required: true,
  },
  {
    id: 'birth',
    label: 'Acta de nacimiento',
    hintText: 'Acta de nacimiento: colócala plana, con las cuatro esquinas visibles.',
    required: true,
  },
  {
    id: 'baptism',
    label: 'Certificado de bautismo',
    hintText: 'Certificado de bautismo: colócalo plano, con las cuatro esquinas visibles.',
    required: true,
  },
  {
    id: 'address',
    label: 'Comprobante de domicilio',
    hintText: 'Comprobante de domicilio: colócalo plano, con las cuatro esquinas visibles.',
    required: false,
  },
]

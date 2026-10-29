import { Plus } from 'lucide-react'
import { Button } from '@/components/ui/button'

type StartScreenProps = { onStart: () => void; onExit: () => void }

export function StartScreen({ onStart, onExit }: StartScreenProps) {
  return (
    <main className="fixed inset-0 z-40 grid place-content-center justify-items-center gap-4 bg-background px-6 text-center text-foreground">
      <h1 className="m-0 font-heading text-2xl">Subir documentos</h1>
      <p className="m-0 max-w-xs text-sm text-muted-foreground">
        Toma una foto de cada documento del estudiante: hoja de inscripción, acta de nacimiento, certificado de bautismo y comprobante de domicilio.
      </p>
      <Button className="h-12 w-full max-w-xs text-base" onClick={onStart}>
        <Plus /> Nuevo estudiante
      </Button>
      <Button variant="link" onClick={onExit}>Volver al panel</Button>
    </main>
  )
}

import { useState } from 'react'
import { toast } from 'sonner'
import { cn } from 'cn'
import {
  AlertCircle,
  Bell,
  BookOpen,
  CalendarDays,
  Check,
  ChevronDown,
  Church,
  CircleHelp,
  ClipboardList,
  LayoutDashboard,
  LogOut,
  MoreHorizontal,
  Plus,
  ScanLine,
  Search,
  Settings2,
  Users,
} from 'lucide-react'
import {
  Alert,
  AlertDescription,
  AlertTitle,
} from './components/ui/alert'
import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from './components/ui/alert-dialog'
import { Avatar, AvatarFallback } from './components/ui/avatar'
import { Badge } from './components/ui/badge'
import { Button } from './components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from './components/ui/card'
import { Checkbox } from './components/ui/checkbox'
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from './components/ui/dialog'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from './components/ui/dropdown-menu'
import { Input } from './components/ui/input'
import { Label } from './components/ui/label'
import {
  Popover,
  PopoverContent,
  PopoverDescription,
  PopoverHeader,
  PopoverTitle,
  PopoverTrigger,
} from './components/ui/popover'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './components/ui/select'
import { Separator } from './components/ui/separator'
import { Skeleton } from './components/ui/skeleton'
import { Switch } from './components/ui/switch'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from './components/ui/table'
import { Tabs, TabsList, TabsTrigger } from './components/ui/tabs'
import { Textarea } from './components/ui/textarea'
import { Tooltip, TooltipContent, TooltipTrigger } from './components/ui/tooltip'
import { AdminAuthGate } from './components/AdminAuthGate'
import { DocumentWizardPage } from './components/doc-wizard/DocumentWizardPage'
import { useHashRoute } from './lib/useHashRoute'
import './App.css'

const DOCUMENT_WIZARD_HASH = '#/subir-documentos'

type AdminSession = { displayName: string; email: string }

function Dashboard({ admin, onLogout }: { admin: AdminSession; onLogout: () => Promise<void> }) {
  const [activeSection, setActiveSection] = useState('Resumen')
  const [activeTab, setActiveTab] = useState('Todos')
  const [search, setSearch] = useState('')
  const initials = admin.displayName.split(/\s+/).slice(0, 2).map((part) => part[0]).join('').toUpperCase()

  const navigation = [
    { label: 'Resumen', icon: LayoutDashboard },
    { label: 'Estudiantes', icon: Users },
    { label: 'Inscripciones', icon: ClipboardList },
    { label: 'Calendario', icon: CalendarDays },
    { label: 'Subir documentos', icon: ScanLine, href: DOCUMENT_WIZARD_HASH },
  ]

  const swatches = [
    { name: 'Verde esmeralda', token: 'primary', shade: 'emerald-800', className: 'bg-emerald-800 text-white' },
    { name: 'Piedra clara', token: 'background', shade: 'stone-50', className: 'bg-stone-50' },
    { name: 'Blanco', token: 'card', shade: 'white', className: 'border-b border-border bg-white' },
    { name: 'Carbón', token: 'foreground', shade: 'stone-900', className: 'bg-stone-900 text-white' },
    { name: 'Ámbar', token: 'highlight', shade: 'amber-800', className: 'bg-amber-800 text-white' },
  ]

  const students = [
    { initials: 'MG', name: 'María García', detail: '3.er año · Grupo San José', status: 'Inscrita', tone: 'status-success' },
    { initials: 'JL', name: 'José López', detail: '2.º año · Grupo Santa María', status: 'Pendiente', tone: 'status-pending' },
  ].filter((student) => {
    const matchesSearch = student.name.toLowerCase().includes(search.toLowerCase())
    const matchesTab = activeTab === 'Todos' || student.status === (activeTab === 'Inscritos' ? 'Inscrita' : 'Pendiente')
    return matchesSearch && matchesTab
  })

  return (
    <div className="min-h-screen text-foreground">
      <aside className="sidebar">
        <a className="flex items-center gap-2.5 px-2 text-foreground no-underline" href="#inicio" aria-label="MariaDigital, inicio">
          <span className="grid size-[34px] shrink-0 place-items-center rounded-[10px] bg-primary text-primary-foreground"><Church size={20} strokeWidth={1.8} /></span>
          <span className="flex flex-col text-sm leading-[1.15] font-semibold">Maria<span className="text-[11px] font-normal text-muted-foreground">Digital</span></span>
        </a>

        <div className="my-8 flex min-w-0 items-center gap-2 rounded-lg border border-border bg-card px-2 py-2.5 max-[760px]:hidden">
          <span className="grid size-8 shrink-0 place-items-center rounded-md bg-secondary text-secondary-foreground"><Church size={16} /></span>
          <span className="flex min-w-0 flex-1 flex-col gap-0.5 [&_strong]:truncate [&_strong]:text-[11px] [&_strong]:font-semibold [&_small]:text-[10px] [&_small]:text-muted-foreground"><strong>Ntra. Sra. de Guadalupe</strong><small>Juan José Ríos, Sin.</small></span>
          <ChevronDown className="shrink-0 text-muted-foreground" size={15} aria-hidden="true" />
        </div>

        <span className="mx-2 mb-2.5 text-[9px] font-semibold tracking-[0.08em] text-muted-foreground max-[760px]:hidden">ADMINISTRACIÓN</span>
        <nav className="main-nav" aria-label="Navegación principal">
          {navigation.map(({ label, icon: Icon, href }) => (
            <Button
              variant="ghost"
              className={cn(
                'flex min-h-[38px] w-full items-center gap-[11px] rounded-md px-2.5 text-left text-xs text-muted-foreground transition-colors hover:bg-muted hover:text-foreground max-[760px]:min-h-[39px] max-[760px]:w-auto max-[760px]:min-w-max max-[760px]:justify-center max-[760px]:gap-[7px] max-[760px]:px-[9px] max-[760px]:text-[10px]',
                activeSection === label && 'bg-accent font-semibold text-primary',
              )}
              key={label}
              onClick={() => { if (href) window.location.hash = href; else setActiveSection(label) }}
              aria-current={activeSection === label ? 'page' : undefined}
            >
              <Icon size={18} strokeWidth={1.8} />
              <span>{label}</span>
              {label === 'Inscripciones' && <span className="ml-auto grid size-5 place-items-center rounded-md bg-secondary text-[10px] font-semibold text-secondary-foreground max-[760px]:size-[17px]">4</span>}
            </Button>
          ))}
        </nav>

        <div className="sidebar-bottom">
          <Button variant="ghost" className="flex min-h-[38px] w-full items-center justify-start gap-[11px] rounded-md px-2.5 text-xs text-muted-foreground"><CircleHelp size={18} strokeWidth={1.8} /><span>Ayuda</span></Button>
          <Button variant="ghost" className="flex min-h-[38px] w-full items-center justify-start gap-[11px] rounded-md px-2.5 text-xs text-muted-foreground"><Settings2 size={18} strokeWidth={1.8} /><span>Configuración</span></Button>
          <div className="mt-3 flex items-center gap-2 border-t border-border px-0.5 pt-3.5">
            <span className="grid size-[30px] shrink-0 place-items-center rounded-full border border-border bg-accent text-[10px] font-semibold text-primary">{initials}</span>
            <span className="flex min-w-0 flex-1 flex-col gap-0.5 [&_strong]:truncate [&_strong]:text-[11px] [&_strong]:font-semibold [&_small]:text-[10px] [&_small]:text-muted-foreground"><strong>{admin.displayName}</strong><small>Administrador</small></span>
            <Button variant="ghost" size="icon-sm" aria-label="Cerrar sesión" title="Cerrar sesión" onClick={() => { void onLogout().catch(() => toast.error('No se pudo cerrar la sesión.')) }}><LogOut size={16} /></Button>
          </div>
        </div>
      </aside>

      <div className="workspace" id="inicio">
        <header className="topbar">
          <div className="flex items-center gap-2.5 text-[11px] text-muted-foreground"><span>MariaDigital</span><span>/</span><strong className="font-medium text-foreground">Guía visual</strong></div>
          <div className="flex items-center gap-4">
            <span className="text-[11px] text-muted-foreground max-[760px]:hidden">Lunes, 27 de octubre</span>
            <Button variant="ghost" size="icon" className="relative text-muted-foreground" aria-label="Notificaciones">
              <Bell size={18} strokeWidth={1.8} /><span className="absolute top-1.5 right-1.5 size-1.5 rounded-full border border-background bg-highlight" />
            </Button>
            <span className="grid size-[30px] shrink-0 place-items-center rounded-full border border-border bg-accent text-[10px] font-semibold text-primary">{initials}</span>
          </div>
        </header>

        <main className="main-content">
          <section className="page-heading">
            <div>
              <div className="eyebrow"><span className="eyebrow-rule" />SISTEMA DE DISEÑO <span className="eyebrow-version">v1.0</span></div>
              <h1>Una identidad serena,<br className="title-break" /> al servicio de la comunidad.</h1>
              <p>Color, tipografía y componentes para una experiencia clara y cercana.</p>
            </div>
            <Button variant="secondary" className="max-[760px]:hidden"><Settings2 size={16} /> Personalizar tema</Button>
          </section>

          <section className="showcase-section palette-section" aria-labelledby="palette-title">
            <div className="section-heading">
              <div><span className="section-index">01</span><h2 id="palette-title">Paleta de color</h2></div>
              <span className="section-note">Calidez, confianza y claridad</span>
            </div>
            <div className="palette-grid">
              {swatches.map((swatch) => (
                <article className="swatch-card" key={swatch.token}>
                  <div className={`swatch-color ${swatch.className}`}>
                    {swatch.token === 'primary' && <Church size={20} strokeWidth={1.6} />}
                    {swatch.token === 'highlight' && <span className="text-2xl">✳</span>}
                  </div>
                  <div className="swatch-meta"><strong>{swatch.name}</strong><span>{swatch.shade}</span></div>
                  <span className="token-name">{swatch.token}</span>
                </article>
              ))}
            </div>
          </section>

          <div className="showcase-grid">
            <section className="showcase-section type-section" aria-labelledby="type-title">
              <div className="section-heading">
                <div><span className="section-index">02</span><h2 id="type-title">Tipografía</h2></div>
                <span className="section-note">Contraste con propósito</span>
              </div>
              <div className="type-sample">
                <span className="sample-label">PLAYFAIR DISPLAY · ENCABEZADOS</span>
                <h3>La comunidad crece<br />con cada familia.</h3>
                <span className="sample-divider" />
                <span className="sample-label">GEIST · INTERFAZ Y LECTURA</span>
                <p>Inscripciones, sacramentos y grupos organizados en un solo lugar.</p>
              </div>
            </section>

            <section className="showcase-section component-section" aria-labelledby="components-title">
              <div className="section-heading">
                <div><span className="section-index">03</span><h2 id="components-title">Componentes</h2></div>
                <span className="section-note">Radio base · 8 px</span>
              </div>
              <div className="component-samples">
                <div className="flex items-center gap-2">
                  <Button onClick={() => toast.success('Formulario de inscripción listo')}><Plus /> Nueva inscripción</Button>
                  <Button variant="outline"><BookOpen /> Ver registros</Button>
                </div>
                <div className="flex h-[35px] min-w-0 items-center gap-2 rounded-md border border-input px-2 text-muted-foreground focus-within:border-ring focus-within:ring-2 focus-within:ring-ring/20">
                  <Search size={17} strokeWidth={1.8} aria-hidden="true" />
                  <Input className="h-full min-w-0 border-0 px-0 text-[11px] shadow-none focus-visible:border-0 focus-visible:ring-0" aria-label="Buscar estudiante" placeholder="Buscar estudiante..." value={search} onChange={(event) => setSearch(event.target.value)} />
                  <kbd className="shrink-0 rounded border border-border px-1 py-0.5 font-sans text-[9px] text-muted-foreground">⌘ K</kbd>
                </div>
                <div className="flex flex-wrap items-center gap-2">
                  <Badge><Check /> Inscrita</Badge>
                  <Badge variant="secondary">Pendiente</Badge>
                  <Badge variant="outline">Archivada</Badge>
                </div>
              </div>
            </section>
          </div>

          <section className="showcase-section records-section" aria-labelledby="records-title">
            <div className="section-heading records-heading">
              <div><span className="section-index">04</span><div><h2 id="records-title">Una interfaz real, no solo muestras</h2><p>Los mismos tokens aplicados a una lista de estudiantes.</p></div></div>
              <Button className="min-h-8 max-[760px]:size-[34px] max-[760px]:min-w-[34px] max-[760px]:overflow-hidden max-[760px]:px-0 max-[760px]:text-[0px]" onClick={() => toast.success('Estudiante agregado a la lista')}><Plus /> Agregar estudiante</Button>
            </div>
            <div className="records-toolbar">
              <Tabs className="h-[43px] justify-start" value={activeTab} onValueChange={(value) => setActiveTab(String(value))}>
                <TabsList variant="line" className="h-full gap-4 p-0" aria-label="Filtrar estudiantes">
                  <TabsTrigger className="flex-none rounded-none px-0.5 text-[10px] after:bottom-[-1px] after:h-0.5 after:bg-primary data-active:text-primary max-[760px]:text-[9px]" value="Todos">Todos</TabsTrigger>
                  <TabsTrigger className="flex-none rounded-none px-0.5 text-[10px] after:bottom-[-1px] after:h-0.5 after:bg-primary data-active:text-primary max-[760px]:text-[9px]" value="Inscritos">Inscritos</TabsTrigger>
                  <TabsTrigger className="flex-none rounded-none px-0.5 text-[10px] after:bottom-[-1px] after:h-0.5 after:bg-primary data-active:text-primary max-[760px]:text-[9px]" value="Pendientes">Pendientes</TabsTrigger>
                </TabsList>
              </Tabs>
              <Popover>
                <PopoverTrigger render={<Button variant="outline" size="sm"><Settings2 /> Filtros</Button>} />
                <PopoverContent>
                  <PopoverHeader>
                    <PopoverTitle>Filtros de estudiantes</PopoverTitle>
                    <PopoverDescription>Limita los registros visibles.</PopoverDescription>
                  </PopoverHeader>
                  <Label className="filter-check"><Checkbox defaultChecked /> Mostrar grupos activos</Label>
                </PopoverContent>
              </Popover>
            </div>
            <div className="student-table-wrap">
              <Table className="w-full min-w-[485px] border-collapse text-left">
                <TableHeader><TableRow><TableHead className="h-[35px] text-[8px] font-semibold tracking-wide text-muted-foreground">ESTUDIANTE</TableHead><TableHead className="h-[35px] text-[8px] font-semibold tracking-wide text-muted-foreground">GRUPO</TableHead><TableHead className="h-[35px] text-[8px] font-semibold tracking-wide text-muted-foreground">ESTADO</TableHead><TableHead className="h-[35px] w-9 text-right text-[8px]" aria-label="Acciones" /></TableRow></TableHeader>
                <TableBody>
                  {students.map((student) => (
                    <TableRow key={student.initials}>
                      <TableCell className="h-[49px] border-t border-border/60"><div className="flex items-center gap-2 whitespace-nowrap"><Avatar size="sm" className="bg-secondary text-secondary-foreground"><AvatarFallback className="bg-secondary text-secondary-foreground">{student.initials}</AvatarFallback></Avatar><strong className="text-[10px] font-medium">{student.name}</strong></div></TableCell>
                      <TableCell className="h-[49px] border-t border-border/60 text-[9px] whitespace-nowrap text-muted-foreground">{student.detail}</TableCell>
                      <TableCell className="h-[49px] border-t border-border/60"><Badge variant={student.tone === 'status-success' ? 'default' : 'secondary'}>{student.tone === 'status-success' && <Check />}{student.status}</Badge></TableCell>
                      <TableCell className="h-[49px] w-9 border-t border-border/60 text-right">
                        <DropdownMenu>
                          <DropdownMenuTrigger render={<Button variant="ghost" size="icon-sm" aria-label={`Más opciones para ${student.name}`}><MoreHorizontal /></Button>} />
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem onClick={() => toast.info(`Abriendo ficha de ${student.name}`)}>Ver ficha</DropdownMenuItem>
                            <DropdownMenuItem onClick={() => toast.success('Inscripción actualizada')}>Editar inscripción</DropdownMenuItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem variant="destructive">Archivar</DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                  ))}
                  {students.length === 0 && <TableRow><TableCell className="px-2 py-5 text-center text-[11px] text-muted-foreground" colSpan={4}>No hay estudiantes que coincidan con la búsqueda.</TableCell></TableRow>}
                </TableBody>
              </Table>
            </div>
            <div className="table-footer"><span>Mostrando {students.length} de 128 estudiantes</span><Button variant="link" size="sm" className="h-auto px-0 text-[11px]">Ver todos <span aria-hidden="true">→</span></Button></div>
          </section>

          <section className="showcase-section mt-[38px] max-[760px]:mt-[30px]" aria-labelledby="primitive-title">
            <div className="section-heading">
              <div><span className="section-index">05</span><div><h2 id="primitive-title">Catálogo de componentes base</h2><p className="mt-1 mb-0 text-[11px] text-muted-foreground">Primitivas compartidas, listas para usar en los flujos de la parroquia.</p></div></div>
              <span className="section-note">21 componentes</span>
            </div>
            <div className="grid grid-cols-1 border-l border-t border-border bg-card min-[761px]:grid-cols-2 min-[1051px]:grid-cols-3">
              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Button · Badge</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">Acciones y estados compactos</p>
                <div className="flex w-full min-w-0 flex-1 flex-wrap content-center items-center gap-2">
                  <Button size="sm"><Plus /> Crear</Button>
                  <Button size="sm" variant="outline">Secundaria</Button>
                  <Button size="sm" variant="ghost">Ligera</Button>
                  <Badge variant="secondary">En revisión</Badge>
                  <Badge variant="outline">Borrador</Badge>
                </div>
              </article>

              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Card</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">Superficie para contenido relacionado</p>
                <Card className="w-full gap-2 shadow-none">
                  <CardHeader className="pb-0"><CardTitle className="text-xs">Grupo San José</CardTitle><CardDescription className="text-[10px]">Inscripciones abiertas</CardDescription></CardHeader>
                  <CardContent className="text-[10px] text-muted-foreground">18 estudiantes registrados</CardContent>
                </Card>
              </article>

              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Input · Label · Textarea</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">Campos claros y etiquetados</p>
                <div className="flex w-full min-w-0 flex-1 flex-col justify-center gap-1.5">
                  <Label className="text-[10px]" htmlFor="sample-email">Correo de contacto</Label>
                  <Input className="text-[11px]" id="sample-email" placeholder="familia@correo.mx" />
                  <Label className="text-[10px]" htmlFor="sample-note">Nota para el equipo</Label>
                  <Textarea className="text-[11px]" id="sample-note" placeholder="Escribe una nota breve..." rows={2} />
                </div>
              </article>

              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Checkbox · Switch</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">Preferencias accesibles</p>
                <div className="flex w-full min-w-0 flex-1 flex-col justify-center gap-1.5">
                  <Label className="min-h-[26px] text-[10px]"><Checkbox defaultChecked /> Recibir avisos de inscripción</Label>
                  <Label className="min-h-[26px] text-[10px]"><Switch defaultChecked /> Recordatorios semanales</Label>
                </div>
              </article>

              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Select</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">Elegir grupo de catequesis</p>
                <Select defaultValue="familias">
                  <SelectTrigger aria-label="Grupo de catequesis" className="mt-2 w-full"><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="familias">Pastoral familiar</SelectItem>
                    <SelectItem value="jovenes">Grupo juvenil</SelectItem>
                    <SelectItem value="catequesis">Catequesis infantil</SelectItem>
                  </SelectContent>
                </Select>
              </article>

              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Avatar</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">Identidad para listas y perfiles</p>
                <div className="flex w-full min-w-0 flex-1 items-center gap-2.5">
                  <Avatar><AvatarFallback>MG</AvatarFallback></Avatar>
                  <Avatar size="lg"><AvatarFallback>JL</AvatarFallback></Avatar>
                  <span className="text-[11px] text-muted-foreground">María García</span>
                </div>
              </article>

              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Tabs</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">Navegación entre vistas cercanas</p>
                <Tabs defaultValue="familias" className="mt-2">
                  <TabsList><TabsTrigger value="familias">Familias</TabsTrigger><TabsTrigger value="grupos">Grupos</TabsTrigger></TabsList>
                </Tabs>
              </article>

              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Table</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">Datos tabulares compactos</p>
                <Table className="mt-1 text-[10px]">
                  <TableHeader><TableRow><TableHead className="h-[30px] px-2 py-1 text-[10px]">Grupo</TableHead><TableHead className="h-[30px] px-2 py-1 text-[10px]">Miembros</TableHead></TableRow></TableHeader>
                  <TableBody><TableRow><TableCell className="h-[30px] px-2 py-1 text-[10px]">San José</TableCell><TableCell className="h-[30px] px-2 py-1 text-[10px]">18</TableCell></TableRow></TableBody>
                </Table>
              </article>

              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Alert</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">Contexto persistente en pantalla</p>
                <Alert className="mt-1 w-full">
                  <AlertCircle />
                  <AlertTitle className="text-[10px]">Inscripción guardada</AlertTitle>
                  <AlertDescription className="text-[10px]">La información ya está disponible para el equipo.</AlertDescription>
                </Alert>
              </article>

              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Dialog</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">Detalles en una ventana enfocada</p>
                <Dialog>
                  <DialogTrigger render={<Button size="sm" variant="outline">Abrir detalles</Button>} />
                  <DialogContent>
                    <DialogHeader>
                      <DialogTitle>Datos de la inscripción</DialogTitle>
                      <DialogDescription>María García · Grupo San José</DialogDescription>
                    </DialogHeader>
                    <Separator />
                    <p className="m-0 text-muted-foreground">Inscripción confirmada para el ciclo 2026.</p>
                    <DialogFooter><DialogClose render={<Button variant="outline">Cerrar</Button>} /></DialogFooter>
                  </DialogContent>
                </Dialog>
              </article>

              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Alert Dialog</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">Confirmación antes de una acción</p>
                <AlertDialog>
                  <AlertDialogTrigger render={<Button size="sm" variant="outline">Revisar confirmación</Button>} />
                  <AlertDialogContent>
                    <AlertDialogHeader>
                      <AlertDialogTitle>Archivar este registro</AlertDialogTitle>
                      <AlertDialogDescription>El estudiante dejará de aparecer en las listas activas.</AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                      <AlertDialogCancel>Cancelar</AlertDialogCancel>
                      <Button variant="destructive" onClick={() => toast.success('Registro archivado')}>Archivar</Button>
                    </AlertDialogFooter>
                  </AlertDialogContent>
                </AlertDialog>
              </article>

              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Dropdown Menu</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">Acciones relacionadas en un menú</p>
                <DropdownMenu>
                  <DropdownMenuTrigger render={<Button size="sm" variant="outline">Acciones <ChevronDown /></Button>} />
                  <DropdownMenuContent>
                    <DropdownMenuItem onClick={() => toast.info('Ficha abierta')}>Ver ficha</DropdownMenuItem>
                    <DropdownMenuItem onClick={() => toast.info('Edición iniciada')}>Editar</DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem variant="destructive">Archivar</DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              </article>

              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Popover</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">Información breve junto a un control</p>
                <Popover>
                  <PopoverTrigger render={<Button size="sm" variant="outline">Ver horario</Button>} />
                  <PopoverContent>
                    <PopoverHeader><PopoverTitle>Próxima reunión</PopoverTitle><PopoverDescription>Sábado, 10:00 · Salón parroquial</PopoverDescription></PopoverHeader>
                  </PopoverContent>
                </Popover>
              </article>

              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Tooltip</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">Ayuda contextual para una acción</p>
                <Tooltip>
                  <TooltipTrigger render={<Button variant="outline" size="icon" aria-label="Ayuda"><CircleHelp /></Button>} />
                  <TooltipContent>Consulta los requisitos de inscripción</TooltipContent>
                </Tooltip>
              </article>

              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Separator</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">División sutil entre contenidos</p>
                <div className="mt-3 flex w-full items-center gap-2 text-[10px] text-muted-foreground"><span>Datos de la familia</span><Separator className="flex-1" /><span>Contacto</span></div>
              </article>

              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Skeleton</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">Estado de carga con forma estable</p>
                <div className="mt-2 flex w-full items-center gap-2.5"><Skeleton className="size-[34px] rounded-full" /><div className="grid flex-1 gap-2"><Skeleton className="h-2 w-full" /><Skeleton className="h-2 w-[62%]" /></div></div>
              </article>

              <article className="flex min-w-0 min-h-[165px] flex-col items-start gap-2 border-b border-r border-border bg-card p-4">
                <h3 className="m-0 font-sans text-[11px] font-semibold">Sonner</h3>
                <p className="-mt-1 mb-0.5 text-[10px] text-muted-foreground">Confirmación temporal de una acción</p>
                <Button size="sm" variant="outline" onClick={() => toast.success('Cambios guardados correctamente')}><Check /> Probar notificación</Button>
              </article>
            </div>
          </section>

          <footer className="page-footer"><span>MARIA DIGITAL</span><span>Diseñado para servir mejor.</span><span>2026</span></footer>
        </main>
      </div>
    </div>
  )
}

export default function App() {
  const route = useHashRoute()

  return (
    <AdminAuthGate>
      {(admin, onLogout) => route === DOCUMENT_WIZARD_HASH ? <DocumentWizardPage /> : <Dashboard admin={admin} onLogout={onLogout} />}
    </AdminAuthGate>
  )
}

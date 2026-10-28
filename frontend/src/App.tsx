import { useState } from 'react'
import {
  Bell,
  BookOpen,
  CalendarDays,
  Check,
  ChevronDown,
  Church,
  CircleHelp,
  ClipboardList,
  LayoutDashboard,
  MoreHorizontal,
  Plus,
  Search,
  Settings2,
  Users,
} from 'lucide-react'
import './App.css'

function App() {
  const [activeSection, setActiveSection] = useState('Resumen')
  const [activeTab, setActiveTab] = useState('Todos')
  const [search, setSearch] = useState('')

  const navigation = [
    { label: 'Resumen', icon: LayoutDashboard },
    { label: 'Estudiantes', icon: Users },
    { label: 'Inscripciones', icon: ClipboardList },
    { label: 'Calendario', icon: CalendarDays },
  ]

  const swatches = [
    { name: 'Verde mariano', token: 'primary', hex: '#245B4B', className: 'swatch-primary' },
    { name: 'Lino cálido', token: 'background', hex: '#F6F4ED', className: 'swatch-background' },
    { name: 'Blanco limpio', token: 'card', hex: '#FFFFFF', className: 'swatch-white' },
    { name: 'Carbón', token: 'foreground', hex: '#252B28', className: 'swatch-foreground' },
    { name: 'Oro antiguo', token: 'highlight', hex: '#8A642F', className: 'swatch-highlight' },
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
    <div className="app-shell">
      <aside className="sidebar">
        <a className="brand" href="#inicio" aria-label="Guadalupe Digital, inicio">
          <span className="brand-mark"><Church size={20} strokeWidth={1.8} /></span>
          <span className="brand-name">Guadalupe<span>Digital</span></span>
        </a>

        <div className="parish-switcher">
          <span className="parish-avatar"><Church size={16} /></span>
          <span className="parish-copy"><strong>Ntra. Sra. de Guadalupe</strong><small>Juan José Ríos, Sin.</small></span>
          <ChevronDown size={15} aria-hidden="true" />
        </div>

        <span className="sidebar-label">ADMINISTRACIÓN</span>
        <nav className="main-nav" aria-label="Navegación principal">
          {navigation.map(({ label, icon: Icon }) => (
            <button
              className={`nav-item${activeSection === label ? ' is-active' : ''}`}
              key={label}
              type="button"
              onClick={() => setActiveSection(label)}
              aria-current={activeSection === label ? 'page' : undefined}
            >
              <Icon size={18} strokeWidth={1.8} />
              <span>{label}</span>
              {label === 'Inscripciones' && <span className="nav-count">4</span>}
            </button>
          ))}
        </nav>

        <div className="sidebar-bottom">
          <button className="nav-item" type="button"><CircleHelp size={18} strokeWidth={1.8} /><span>Ayuda</span></button>
          <button className="nav-item" type="button"><Settings2 size={18} strokeWidth={1.8} /><span>Configuración</span></button>
          <div className="account-row">
            <span className="account-avatar">PV</span>
            <span className="account-copy"><strong>Pablo Valdez</strong><small>Administrador</small></span>
            <MoreHorizontal size={18} />
          </div>
        </div>
      </aside>

      <div className="workspace" id="inicio">
        <header className="topbar">
          <div className="breadcrumb"><span>Guadalupe Digital</span><span className="breadcrumb-slash">/</span><strong>Guía visual</strong></div>
          <div className="topbar-actions">
            <span className="topbar-date">Lunes, 27 de octubre</span>
            <button className="icon-button notification-button" type="button" aria-label="Notificaciones">
              <Bell size={18} strokeWidth={1.8} /><span className="notification-dot" />
            </button>
            <span className="topbar-avatar">PV</span>
          </div>
        </header>

        <main className="main-content">
          <section className="page-heading">
            <div>
              <div className="eyebrow"><span className="eyebrow-rule" />SISTEMA DE DISEÑO <span className="eyebrow-version">v1.0</span></div>
              <h1>Una identidad serena,<br className="title-break" /> al servicio de la comunidad.</h1>
              <p>Color, tipografía y componentes para una experiencia clara y cercana.</p>
            </div>
            <button className="button button-secondary settings-button" type="button"><Settings2 size={16} /> Personalizar tema</button>
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
                    {swatch.token === 'highlight' && <span className="gold-star">✳</span>}
                  </div>
                  <div className="swatch-meta"><strong>{swatch.name}</strong><span>{swatch.hex}</span></div>
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
                <div className="component-row button-row">
                  <button className="button button-primary" type="button"><Plus size={16} /> Nueva inscripción</button>
                  <button className="button button-outline" type="button"><BookOpen size={16} /> Ver registros</button>
                </div>
                <label className="search-field">
                  <Search size={17} strokeWidth={1.8} />
                  <input aria-label="Buscar estudiante" placeholder="Buscar estudiante..." value={search} onChange={(event) => setSearch(event.target.value)} />
                  <kbd>⌘ K</kbd>
                </label>
                <div className="status-row">
                  <span className="status-pill status-success"><Check size={13} /> Inscrita</span>
                  <span className="status-pill status-pending"><span className="status-dot" /> Pendiente</span>
                  <span className="status-pill status-neutral">Archivada</span>
                </div>
              </div>
            </section>
          </div>

          <section className="showcase-section records-section" aria-labelledby="records-title">
            <div className="section-heading records-heading">
              <div><span className="section-index">04</span><div><h2 id="records-title">Una interfaz real, no solo muestras</h2><p>Los mismos tokens aplicados a una lista de estudiantes.</p></div></div>
              <button className="button button-primary records-add" type="button"><Plus size={16} /> Agregar estudiante</button>
            </div>
            <div className="records-toolbar">
              <div className="tabs" role="tablist" aria-label="Filtrar estudiantes">
                {['Todos', 'Inscritos', 'Pendientes'].map((tab) => (
                  <button className={`tab${activeTab === tab ? ' is-selected' : ''}`} key={tab} type="button" role="tab" aria-selected={activeTab === tab} onClick={() => setActiveTab(tab)}>{tab}</button>
                ))}
              </div>
              <button className="filter-button" type="button"><Settings2 size={15} /> Filtros</button>
            </div>
            <div className="student-table-wrap">
              <table className="student-table">
                <thead><tr><th>ESTUDIANTE</th><th>GRUPO</th><th>ESTADO</th><th aria-label="Acciones" /></tr></thead>
                <tbody>
                  {students.map((student) => (
                    <tr key={student.initials}>
                      <td><div className="student-name"><span className="student-avatar">{student.initials}</span><strong>{student.name}</strong></div></td>
                      <td className="student-detail">{student.detail}</td>
                      <td><span className={`status-pill ${student.tone}`}>{student.tone === 'status-success' && <Check size={13} />}{student.status}</span></td>
                      <td><button className="icon-button row-action" type="button" aria-label={`Más opciones para ${student.name}`}><MoreHorizontal size={18} /></button></td>
                    </tr>
                  ))}
                  {students.length === 0 && <tr><td className="empty-row" colSpan={4}>No hay estudiantes que coincidan con la búsqueda.</td></tr>}
                </tbody>
              </table>
            </div>
            <div className="table-footer"><span>Mostrando {students.length} de 128 estudiantes</span><button type="button" className="text-action">Ver todos <span aria-hidden="true">→</span></button></div>
          </section>

          <footer className="page-footer"><span>GUADALUPE DIGITAL</span><span>Diseñado para servir mejor.</span><span>2026</span></footer>
        </main>
      </div>
    </div>
  )
}

export default App

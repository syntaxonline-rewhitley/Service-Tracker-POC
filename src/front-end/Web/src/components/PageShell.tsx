interface Props {
  children: React.ReactNode
  title?: string
}

export function PageShell({ children, title }: Props) {
  return (
    <div className="max-w-6xl mx-auto px-4 py-6">
      {title && <h1 className="text-2xl font-bold mb-6 text-gray-800">{title}</h1>}
      {children}
    </div>
  )
}

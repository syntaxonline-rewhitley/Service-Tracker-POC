interface Props {
  message: string
}

export function ErrorMessage({ message }: Props) {
  return (
    <div className="bg-red-50 border border-red-300 text-red-700 rounded px-4 py-3 text-sm">
      {message}
    </div>
  )
}

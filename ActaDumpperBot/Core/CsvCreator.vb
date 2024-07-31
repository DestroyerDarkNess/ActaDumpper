Imports System.IO

Namespace Core
    Public Class CsvCreator

        Private _headers As List(Of String)
        Private _rows As List(Of Dictionary(Of String, String))

        Public Sub New(headers As List(Of String))
            _headers = headers
            _rows = New List(Of Dictionary(Of String, String))()
        End Sub

        ' Método para agregar una fila al CSV
        Public Sub AddRow(row As Dictionary(Of String, String))
            If row.Keys.All(Function(k) _headers.Contains(k)) Then
                _rows.Add(row)
            Else
                Throw New ArgumentException("All keys in the row must be valid headers")
            End If
        End Sub

        ' Método para eliminar una fila del CSV
        Public Sub RemoveRow(index As Integer)
            If index >= 0 And index < _rows.Count Then
                _rows.RemoveAt(index)
            Else
                Throw New ArgumentOutOfRangeException("Index out of range")
            End If
        End Sub

        ' Método para buscar en una columna específica
        Public Function SearchInColumn(column As String, value As String) As List(Of Dictionary(Of String, String))
            If _headers.Contains(column) Then
                Return _rows.Where(Function(row) row(column).Contains(value)).ToList()
            Else
                Throw New ArgumentException("Invalid column name")
            End If
        End Function

        ' Método para guardar el CSV a un archivo
        Public Sub SaveToFile(filePath As String)
            Try
                Using writer As New StreamWriter(filePath)
                    writer.WriteLine(String.Join(",", _headers))

                    For Each row As Dictionary(Of String, String) In _rows
                        Dim values As New List(Of String)
                        For Each header As String In _headers
                            values.Add(row(header))
                        Next
                        writer.WriteLine(String.Join(",", values))
                    Next
                End Using
            Catch ex As Exception : End Try
        End Sub

        ' Método para cargar un CSV desde un archivo
        Public Sub LoadFromFile(filePath As String)
            Try
                Using reader As New StreamReader(filePath)
                    _headers = reader.ReadLine().Split(","c).ToList()

                    _rows.Clear()
                    While Not reader.EndOfStream
                        Dim values = reader.ReadLine().Split(","c)
                        Dim row As New Dictionary(Of String, String)
                        For i As Integer = 0 To _headers.Count - 1
                            row(_headers(i)) = values(i)
                        Next
                        _rows.Add(row)
                    End While
                End Using
            Catch ex As Exception : End Try
        End Sub

        ' Método para obtener todas las filas del CSV
        Public Function GetAllRows() As List(Of Dictionary(Of String, String))
            Return _rows
        End Function
    End Class


End Namespace
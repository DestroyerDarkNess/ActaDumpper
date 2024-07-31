Imports System.Runtime.InteropServices
Imports System.Threading

Namespace Core
    Public Class ThreadPlus
        Implements IDisposable

        Private ManualReset As New ManualResetEvent(False)

        Private _Thread As Thread = Nothing

        Public ReadOnly Property Thread As Thread
            Get
                Return _Thread
            End Get
        End Property

        Public ReadOnly Property IsAlive As Boolean
            Get
                Return _Thread.IsAlive
            End Get
        End Property

        Public ReadOnly Property IsBackground As Boolean
            Get
                Return _Thread.IsBackground
            End Get
        End Property

        Public ReadOnly Property IsThreadPoolThread As Boolean
            Get
                Return _Thread.IsThreadPoolThread
            End Get
        End Property

        Public Sub New(ByVal start As ThreadStart)
            _Thread = New Thread(start)
            _Thread.TrySetApartmentState(ApartmentState.MTA)
            _Thread.Priority = ThreadPriority.Normal
        End Sub

        Public Sub New(ByVal start As ThreadStart, ByVal maxStackSize As Integer)
            _Thread = New Thread(start, maxStackSize)
        End Sub

        Public Sub New(ByVal start As ParameterizedThreadStart)
            _Thread = New Thread(start)
        End Sub

        Public Sub New(ByVal start As ParameterizedThreadStart, ByVal maxStackSize As Integer)
            _Thread = New Thread(start, maxStackSize)
        End Sub

        Public Sub Start()
            If Not _Thread.IsAlive Then _Thread.Start()
        End Sub
        Public Sub Join()
            _Thread.Join()
        End Sub

        Public Sub PauseThread()
            If _Thread.IsAlive Then _Thread.Suspend()
        End Sub

        Public Sub ResumeThread()
            If _Thread.IsAlive Then _Thread.Resume()
        End Sub

        Public Sub Cancel()
            _Thread.Abort()
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _Thread IsNot Nothing Then
                Try
                    _Thread.Abort()
                Catch ex As Exception : End Try
                GC.SuppressFinalize(_Thread)
            End If
            GC.SuppressFinalize(Me)
        End Sub

    End Class

End Namespace


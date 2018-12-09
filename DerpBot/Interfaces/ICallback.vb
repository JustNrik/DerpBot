Imports Discord

Public Interface ICallback
    Function DisplayAsync() As Task
    ReadOnly Property Message As IUserMessage
End Interface
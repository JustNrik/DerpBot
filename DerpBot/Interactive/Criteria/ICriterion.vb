Imports Discord.Commands

Public Interface ICriterion(Of In T)
    Function JudgeAsync(sourceContext As ICommandContext, parameter As T) As Task(Of Boolean)
End Interface

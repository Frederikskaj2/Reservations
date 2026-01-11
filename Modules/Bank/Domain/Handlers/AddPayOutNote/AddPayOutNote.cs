namespace Frederikskaj2.Reservations.Bank;

static class AddPayOutNote
{
    public static AddPayOutNoteOutput AddPayOutNoteCore(AddPayOutNoteInput input) =>
        new(AddNote(input.PayOut, input.Command));

    static PayOut AddNote(PayOut payOut, AddPayOutNoteCommand command) =>
        payOut with
        {
            Notes = payOut.Notes.Add(CreateNote(command)),
            Audits = payOut.Audits.Add(CreateAudit(command)),
        };

    static PayOutNote CreateNote(AddPayOutNoteCommand command) =>
        new(command.Timestamp, command.UserId, command.Text);

    static PayOutAudit CreateAudit(AddPayOutNoteCommand command) =>
        new(command.Timestamp, command.UserId, PayOutAuditType.AddNote);
}

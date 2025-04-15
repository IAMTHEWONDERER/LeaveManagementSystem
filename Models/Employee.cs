//employee model
public class Employee
{
    public int Id {get; set;}
    public string FullName{get;set;}
    public string Department {get;set;}
    public DateTime JoiningDate {get;set;}
    public ICollection<LeaveRequest> LeaveRequests {get;set;}
}

public enum LeaveType
{
    Annual,
    Other
}
public enum LeaveStatus
{
    Pending,
    Approved,
    Denied
}

//Leave request model
public class LeaveRequest
{
    public int Id {get; set;}
    public int EmployeeId {get; set;}
    public LeaveType LeaveType {get;set;}
    public DateTime StartDate {get;set;}
    public DateTime EndDate {get;set;}
    public LeaveStatus Status {get;set;}
    public  string Reason {get;set;}
    public DateTime CreatedAt {get;set;}
    public Employee Employee {get;set;}
}

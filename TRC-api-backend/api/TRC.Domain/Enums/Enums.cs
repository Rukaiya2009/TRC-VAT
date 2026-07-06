namespace TRC.Domain.Enums;

public enum UserRole { Admin = 1, Auditor = 2, Prospect = 3, Importer = 4 }

public enum ImportStatus { Draft = 0, Submitted = 1, Assessed = 2 }

public enum RiskLevel { Low = 0, Medium = 1, High = 2 }

// Tax lines assessed on a Bill of Entry (Section 8).
public enum TaxType { CD = 1, RD = 2, SD = 3, VAT = 4, AIT = 5, AT = 6, ATV = 7 }

public enum AppointmentStatus { Booked = 0, Confirmed = 1, Completed = 2, Missed = 3, Cancelled = 4 }

public enum Language { En = 1, Bn = 2 }

public enum Channel { WhatsApp = 1, Email = 2 }

// Phase A = live at launch; Phase B = activates as client data arrives (SRS §9).
public enum RulePhase { A = 1, B = 2 }

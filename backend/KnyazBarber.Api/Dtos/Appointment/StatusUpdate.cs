using System.ComponentModel.DataAnnotations;
using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Dtos.Appointment;

public record StatusUpdate([Required] AppointmentStatus Status);

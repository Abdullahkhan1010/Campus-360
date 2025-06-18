// filepath: d:\projects\Campus 360\Campus360\Services\CalendarService.cs
using Campus360.Models;

namespace Campus360.Services
{    /// <summary>
    /// Comprehensive Calendar Service for Campus360 Academic Timeline System
    /// Manages events, schedules, and integrates with automation engine
    /// </summary>
    public class CalendarService
    {
        private readonly HttpClient _httpClient;
        private readonly AutomationEngineService _automationEngine;
        private readonly StudentServiceEnhanced _studentService;
        private readonly TeacherServiceEnhanced _teacherService;
        private readonly ActivityLogService _activityLogService;
        private readonly AdminService _adminService;
        private readonly DatabaseService _databaseService;
        private readonly ILogger<CalendarService> _logger;

        public CalendarService(
            HttpClient httpClient,
            AutomationEngineService automationEngine,
            StudentServiceEnhanced studentService,
            TeacherServiceEnhanced teacherService,
            ActivityLogService activityLogService,
            AdminService adminService,
            DatabaseService databaseService,
            ILogger<CalendarService> logger)
        {
            _httpClient = httpClient;
            _automationEngine = automationEngine;
            _studentService = studentService;
            _teacherService = teacherService;
            _activityLogService = activityLogService;
            _adminService = adminService;
            _databaseService = databaseService;
            _logger = logger;
        }

        // =============== PUBLIC API METHODS ===============

        /// <summary>
        /// Get calendar view for specific user and date range
        /// </summary>
        public async Task<CalendarViewModel> GetCalendarViewAsync(string userId, string userRole, CalendarViewType viewType = CalendarViewType.Month, DateTime? targetDate = null)
        {
            var currentDate = targetDate ?? DateTime.Now;
            var (startDate, endDate) = GetDateRangeForView(currentDate, viewType);
            
            await Task.Delay(300); // Simulate API call
            
            // Get user-specific events
            var userEvents = await GetUserEventsAsync(userId, userRole, startDate, endDate);
            
            // Get automation-generated events
            var automationEvents = await GetAutomationEventsAsync(userId, userRole, startDate, endDate);
            
            // Combine and sort events
            var allEvents = userEvents.Concat(automationEvents)
                .OrderBy(e => e.StartDate)
                .ThenBy(e => e.StartTime)
                .ToList();
            
            var viewModel = new CalendarViewModel
            {
                CurrentDate = currentDate,
                ViewType = viewType,
                Events = allEvents,
                UserId = userId,
                UserRole = userRole,
                StartDate = startDate,
                EndDate = endDate,
                TotalEvents = allEvents.Count,
                TodayEvents = allEvents.Count(e => e.StartDate.Date == DateTime.Today),
                UpcomingEvents = allEvents.Count(e => e.StartDate > DateTime.Now),
                OverdueEvents = allEvents.Count(e => e.StartDate < DateTime.Now && e.EventType == AcademicEventType.Assignment && e.Status != EventStatus.Completed)
            };
            
            return viewModel;
        }

        /// <summary>
        /// Get timeline view for user (past and upcoming events)
        /// </summary>
        public async Task<List<TimelineGroup>> GetTimelineAsync(string userId, string userRole, int pastDays = 30, int futureDays = 90)
        {
            await Task.Delay(300);
            
            var startDate = DateTime.Now.AddDays(-pastDays);
            var endDate = DateTime.Now.AddDays(futureDays);
            
            var events = await GetUserEventsAsync(userId, userRole, startDate, endDate);
            var timelineEvents = events.Select(MapToTimelineEvent).OrderByDescending(e => e.Date).ToList();
            
            return GroupEventsByDate(timelineEvents);
        }

        /// <summary>
        /// Get today's events for user
        /// </summary>
        public async Task<List<AcademicEvent>> GetTodayEventsAsync(string userId, string userRole)
        {
            await Task.Delay(200);
            
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            return await GetUserEventsAsync(userId, userRole, today, tomorrow);
        }

        /// <summary>
        /// Get upcoming events for user (next N days)
        /// </summary>
        public async Task<List<AcademicEvent>> GetUpcomingEventsAsync(string userId, string userRole, int days = 7)
        {
            await Task.Delay(200);
            
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(days);
            
            var events = await GetUserEventsAsync(userId, userRole, startDate, endDate);
            return events.Where(e => e.StartDate > DateTime.Now).OrderBy(e => e.StartDate).ToList();
        }        /// <summary>
        /// Create new academic event (Admin/Teacher only)
        /// </summary>
        public async Task<SaveResult> CreateEventAsync(CreateEventModel model, string createdBy, string createdByRole)
        {
            try
            {
                var academicEvent = new AcademicEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = model.Title,
                    Description = model.Description,
                    EventType = model.EventType,
                    Category = GetCategoryFromEventType(model.EventType),
                    Priority = model.Priority,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    IsAllDay = model.IsAllDay,
                    Venue = model.Venue,
                    CourseId = model.CourseId,
                    TargetRole = model.TargetRole,
                    TargetUserIds = model.TargetUserIds,
                    CreatedBy = createdBy,
                    CreatedByRole = createdByRole,
                    Color = model.Color,
                    Tags = model.Tags,
                    IsRecurring = model.IsRecurring,
                    RecurringPattern = model.RecurringPattern,
                    HasReminder = model.HasReminder,
                    IconClass = GetIconClassForEventType(model.EventType),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = EventStatus.Active,
                    IsPublished = true
                };

                // Create reminders if specified
                if (model.HasReminder && model.ReminderMinutes.Any())
                {
                    academicEvent.Reminders = model.ReminderMinutes.Select(minutes => new EventReminder
                    {
                        Id = Guid.NewGuid().ToString(),
                        EventId = academicEvent.Id,
                        MinutesBefore = minutes,
                        Type = ReminderType.Notification
                    }).ToList();
                }

                // Map to database model and insert
                var eventDb = MapToEventDb(academicEvent);
                var insertedEvent = await _databaseService.InsertAsync(eventDb);
                
                if (insertedEvent == null)
                {
                    return new SaveResult { Success = false, Message = "Failed to create event in database" };
                }

                // Trigger automation notification for event creation
                await _automationEngine.TriggerCustomEventAsync(
                    academicEvent.Id,
                    $"New {model.EventType}",
                    $"{model.Title} has been scheduled for {model.StartDate:MMM dd, yyyy}",
                    model.TargetRole.ToString().ToLower(),
                    createdBy);
                
                // Log activity for audit
                await _activityLogService.LogActivityAsync(
                    createdBy,
                    "Created Calendar Event",
                    $"Created {model.EventType} event '{model.Title}' scheduled for {model.StartDate:d}",
                    "event_created");

                return new SaveResult { Success = true, Message = "Event created successfully!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event for user {UserId}", createdBy);
                return new SaveResult { Success = false, Message = $"Error creating event: {ex.Message}" };
            }
        }        /// <summary>
        /// Update existing event
        /// </summary>
        public async Task<SaveResult> UpdateEventAsync(string eventId, CreateEventModel model, string updatedBy)
        {
            try
            {
                // Get existing event from database
                var existingEvent = await _databaseService.GetEventByIdAsync(eventId);
                if (existingEvent == null)
                {
                    return new SaveResult { Success = false, Message = "Event not found" };
                }

                // Update event properties
                existingEvent.Title = model.Title;
                existingEvent.Description = model.Description;
                existingEvent.StartDate = model.StartDate;
                existingEvent.EndDate = model.EndDate;
                existingEvent.StartTime = model.StartTime;
                existingEvent.EndTime = model.EndTime;
                existingEvent.IsAllDay = model.IsAllDay;
                existingEvent.Venue = model.Venue;
                existingEvent.Priority = model.Priority;
                existingEvent.Color = model.Color;
                existingEvent.Tags = model.Tags;
                existingEvent.UpdatedAt = DateTime.UtcNow;
                existingEvent.UpdatedBy = updatedBy;

                // Map to database model and update
                var eventDb = MapToEventDb(existingEvent);
                var updatedEventDb = await _databaseService.UpdateAsync(eventDb);
                
                if (updatedEventDb == null)
                {
                    return new SaveResult { Success = false, Message = "Failed to update event in database" };
                }
                
                // Log activity for audit
                await _activityLogService.LogActivityAsync(
                    updatedBy,
                    "Updated Calendar Event",
                    $"Updated {existingEvent.EventType} event '{existingEvent.Title}' scheduled for {existingEvent.StartDate:d}",
                    "event_updated");

                return new SaveResult { Success = true, Message = "Event updated successfully!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event {EventId}", eventId);
                return new SaveResult { Success = false, Message = $"Error updating event: {ex.Message}" };
            }
        }        /// <summary>
        /// Delete event
        /// </summary>
        public async Task<SaveResult> DeleteEventAsync(string eventId, string deletedBy)
        {
            try
            {
                // Get existing event from database
                var eventToDelete = await _databaseService.GetEventByIdAsync(eventId);
                if (eventToDelete == null)
                {
                    return new SaveResult { Success = false, Message = "Event not found" };
                }

                // Don't delete system-generated events, mark as cancelled instead
                if (eventToDelete.IsSystemGenerated)
                {
                    eventToDelete.Status = EventStatus.Cancelled;
                    eventToDelete.UpdatedAt = DateTime.UtcNow;
                    eventToDelete.UpdatedBy = deletedBy;
                    
                    // Update in database
                    var eventDb = MapToEventDb(eventToDelete);
                    await _databaseService.UpdateEventAsync(eventDb);
                }
                else
                {
                    // Actually delete the event from database
                    await _databaseService.DeleteEventAsync(eventId);
                }
                
                // Log activity for audit
                await _activityLogService.LogActivityAsync(
                    deletedBy,
                    "Deleted Calendar Event",
                    $"Deleted {eventToDelete.EventType} event '{eventToDelete.Title}' scheduled for {eventToDelete.StartDate:d}",
                    "event_deleted");

                return new SaveResult { Success = true, Message = "Event deleted successfully!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event {EventId}", eventId);
                return new SaveResult { Success = false, Message = $"Error deleting event: {ex.Message}" };
            }
        }
          /// <summary>
        /// Gets courses that are available to a user based on their role
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="userRole">The user's role (Student, Teacher, Admin)</param>
        /// <returns>List of available courses</returns>
        public async Task<List<Course>> GetAvailableCoursesAsync(string userId, string userRole)
        {
            try
            {
                // For simplicity, use AdminService to get all courses
                // In a real implementation, this would filter based on role and enrollment
                return await _adminService.GetCoursesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAvailableCoursesAsync: {ex.Message}");
                return new List<Course>();
            }
        }

        /// <summary>
        /// Get calendar statistics for dashboard
        /// </summary>
        public async Task<CalendarStats> GetCalendarStatsAsync(string userId, string userRole)
        {
            await Task.Delay(200);
            
            var userEvents = await GetUserEventsAsync(userId, userRole, DateTime.Now.AddDays(-30), DateTime.Now.AddDays(90));
            var today = DateTime.Today;
            
            var stats = new CalendarStats
            {
                TotalEvents = userEvents.Count,
                TodayEvents = userEvents.Count(e => e.StartDate.Date == today),
                ThisWeekEvents = userEvents.Count(e => e.StartDate >= today && e.StartDate <= today.AddDays(7)),
                ThisMonthEvents = userEvents.Count(e => e.StartDate.Month == today.Month && e.StartDate.Year == today.Year),
                UpcomingAssignments = userEvents.Count(e => e.EventType == AcademicEventType.Assignment && e.StartDate > DateTime.Now),
                UpcomingExams = userEvents.Count(e => e.EventType == AcademicEventType.Exam && e.StartDate > DateTime.Now),
                OverdueItems = userEvents.Count(e => e.StartDate < DateTime.Now && e.EventType == AcademicEventType.Assignment && e.Status != EventStatus.Completed)
            };
            
            // Group by type
            stats.EventsByType = userEvents.GroupBy(e => e.EventType)
                .ToDictionary(g => g.Key, g => g.Count());
            
            // Group by priority
            stats.EventsByPriority = userEvents.GroupBy(e => e.Priority)
                .ToDictionary(g => g.Key, g => g.Count());
            
            // Recent and upcoming events
            stats.RecentEvents = userEvents
                .Where(e => e.StartDate <= DateTime.Now && e.StartDate >= DateTime.Now.AddDays(-7))
                .Select(MapToTimelineEvent)
                .OrderByDescending(e => e.Date)
                .Take(5)
                .ToList();
                
            stats.UpcomingEvents = userEvents
                .Where(e => e.StartDate > DateTime.Now)
                .Select(MapToTimelineEvent)
                .OrderBy(e => e.Date)
                .Take(5)
                .ToList();
            
            return stats;
        }

        /// <summary>
        /// Sync automation events (assignments, results, notices) with calendar
        /// </summary>
        public async Task<EventSyncResult> SyncAutomationEventsAsync(string userId, string userRole)
        {
            await Task.Delay(500);
            
            try
            {
                var result = new EventSyncResult();
                
                // Sync assignments as events
                if (userRole == "Student")
                {
                    var assignments = await _studentService.GetStudentAssignmentsAsync(userId);
                    result.EventsCreated += await SyncAssignmentEventsAsync(assignments);
                }
                else if (userRole == "Teacher")
                {
                    var assignments = await _teacherService.GetTeacherAssignmentsAsync(userId);
                    result.EventsCreated += await SyncTeacherAssignmentEventsAsync(assignments);
                }
                
                // Sync notices as events
                var notifications = await _automationEngine.GetUserNotificationsAsync(userId);
                result.EventsCreated += await SyncNotificationEventsAsync(notifications);
                
                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                return new EventSyncResult
                {
                    Success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // =============== PRIVATE HELPER METHODS ===============        
        private async Task<List<AcademicEvent>> GetUserEventsAsync(string userId, string userRole, DateTime startDate, DateTime endDate)
        {
            try
            {
                // Get all events from database for the user's role
                var allEvents = await _databaseService.GetEventsForUserAsync(userId, userRole);
                
                // Filter events based on date range and visibility
                var filteredEvents = allEvents.Where(e => 
                    e.StartDate >= startDate && 
                    e.StartDate <= endDate &&
                    e.Status != EventStatus.Cancelled &&
                    IsEventVisibleToUser(e, userId, userRole)
                ).ToList();
                
                return filteredEvents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user events for user {UserId} with role {UserRole}", userId, userRole);
                return new List<AcademicEvent>();
            }
        }

        private async Task<List<AcademicEvent>> GetAutomationEventsAsync(string userId, string userRole, DateTime startDate, DateTime endDate)
        {
            // Get automation-generated events from notifications
            var notifications = await _automationEngine.GetUserNotificationsAsync(userId);
            
            var automationEvents = new List<AcademicEvent>();
            
            foreach (var notification in notifications)
            {
                if (notification.CreatedAt >= startDate && notification.CreatedAt <= endDate)
                {
                    var eventType = MapNotificationTypeToEventType(notification.Type);
                    
                    automationEvents.Add(new AcademicEvent
                    {
                        Id = $"auto_{notification.Id}",
                        Title = notification.Title,
                        Description = notification.Message,
                        EventType = eventType,
                        StartDate = notification.CreatedAt,
                        IsAllDay = true,
                        CourseId = notification.CourseId,
                        CourseName = notification.CourseName,
                        CourseCode = notification.CourseCode,
                        TargetRole = EventTargetRole.Student,
                        IsSystemGenerated = true,
                        SourceType = "Notification",
                        SourceId = notification.Id,
                        Priority = MapNotificationPriorityToEventPriority(notification.Priority),
                        Color = GetColorForEventType(eventType),
                        IconClass = notification.IconClass ?? GetIconClassForEventType(eventType),
                        Status = notification.IsRead ? EventStatus.Completed : EventStatus.Active
                    });
                }
            }
            
            return automationEvents;
        }

        private bool IsEventVisibleToUser(AcademicEvent academicEvent, string userId, string userRole)
        {
            // Check target role
            if (academicEvent.TargetRole == EventTargetRole.All)
                return true;
                
            if (academicEvent.TargetRole.ToString().Equals(userRole, StringComparison.OrdinalIgnoreCase))
                return true;
                
            // Check specific user targeting
            if (academicEvent.TargetUserIds.Contains(userId))
                return true;
                
            // For student role, check course enrollment
            if (userRole == "Student" && !string.IsNullOrEmpty(academicEvent.CourseId))
            {
                // This would normally check if student is enrolled in the course
                return true; // Simplified for mock data
            }
            
            return false;
        }

        private (DateTime startDate, DateTime endDate) GetDateRangeForView(DateTime currentDate, CalendarViewType viewType)
        {
            return viewType switch
            {
                CalendarViewType.Day => (currentDate.Date, currentDate.Date.AddDays(1)),
                CalendarViewType.Week => (GetStartOfWeek(currentDate), GetStartOfWeek(currentDate).AddDays(7)),
                CalendarViewType.Month => (GetStartOfMonth(currentDate), GetStartOfMonth(currentDate).AddMonths(1)),
                CalendarViewType.Timeline => (currentDate.AddDays(-30), currentDate.AddDays(90)),
                CalendarViewType.Agenda => (currentDate.Date, currentDate.Date.AddDays(30)),
                _ => (currentDate.Date, currentDate.Date.AddDays(1))
            };
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private DateTime GetStartOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        private TimelineEvent MapToTimelineEvent(AcademicEvent academicEvent)
        {
            var now = DateTime.Now;
            var eventDate = academicEvent.StartDate;
            
            return new TimelineEvent
            {
                Id = academicEvent.Id,
                Title = academicEvent.Title,
                Description = academicEvent.Description,
                Date = eventDate,
                EventType = academicEvent.EventType,
                Priority = academicEvent.Priority,
                Color = academicEvent.Color,
                IconClass = academicEvent.IconClass ?? GetIconClassForEventType(academicEvent.EventType),
                CourseInfo = !string.IsNullOrEmpty(academicEvent.CourseCode) ? $"{academicEvent.CourseCode} - {academicEvent.CourseName}" : null,
                IsCompleted = academicEvent.Status == EventStatus.Completed,
                IsPast = eventDate < now,
                IsToday = eventDate.Date == now.Date,
                IsUpcoming = eventDate > now,
                TimeAgo = GetTimeAgoString(eventDate),
                Venue = academicEvent.Venue
            };
        }

        private List<TimelineGroup> GroupEventsByDate(List<TimelineEvent> events)
        {
            var groups = new List<TimelineGroup>();
            var now = DateTime.Now;
            
            // Group events by date categories
            var todayEvents = events.Where(e => e.Date.Date == now.Date).ToList();
            var yesterdayEvents = events.Where(e => e.Date.Date == now.Date.AddDays(-1)).ToList();
            var thisWeekEvents = events.Where(e => e.Date >= GetStartOfWeek(now) && e.Date < now.Date.AddDays(-1)).ToList();
            var tomorrowEvents = events.Where(e => e.Date.Date == now.Date.AddDays(1)).ToList();
            var nextWeekEvents = events.Where(e => e.Date > now.Date.AddDays(1) && e.Date <= GetStartOfWeek(now).AddDays(7)).ToList();
            var olderEvents = events.Where(e => e.Date < GetStartOfWeek(now)).ToList();
            var futureEvents = events.Where(e => e.Date > GetStartOfWeek(now).AddDays(7)).ToList();
            
            // Add groups with events
            if (todayEvents.Any())
                groups.Add(new TimelineGroup { GroupTitle = "Today", GroupDate = now.Date, Events = todayEvents, IsToday = true });
                
            if (tomorrowEvents.Any())
                groups.Add(new TimelineGroup { GroupTitle = "Tomorrow", GroupDate = now.Date.AddDays(1), Events = tomorrowEvents, IsUpcoming = true });
                
            if (yesterdayEvents.Any())
                groups.Add(new TimelineGroup { GroupTitle = "Yesterday", GroupDate = now.Date.AddDays(-1), Events = yesterdayEvents, IsPast = true });
                
            if (nextWeekEvents.Any())
                groups.Add(new TimelineGroup { GroupTitle = "Next Week", GroupDate = GetStartOfWeek(now).AddDays(7), Events = nextWeekEvents, IsUpcoming = true });
                
            if (thisWeekEvents.Any())
                groups.Add(new TimelineGroup { GroupTitle = "Earlier This Week", GroupDate = GetStartOfWeek(now), Events = thisWeekEvents, IsPast = true });
                
            if (futureEvents.Any())
                groups.Add(new TimelineGroup { GroupTitle = "Later", GroupDate = futureEvents.First().Date, Events = futureEvents, IsUpcoming = true });
                
            if (olderEvents.Any())
                groups.Add(new TimelineGroup { GroupTitle = "Earlier", GroupDate = olderEvents.First().Date, Events = olderEvents, IsPast = true });
            
            return groups;
        }

        private string GetTimeAgoString(DateTime date)
        {
            var timeSpan = DateTime.Now - date;
            
            if (timeSpan.Days > 0)
                return $"{timeSpan.Days} day{(timeSpan.Days == 1 ? "" : "s")} ago";
            else if (timeSpan.Hours > 0)
                return $"{timeSpan.Hours} hour{(timeSpan.Hours == 1 ? "" : "s")} ago";
            else if (timeSpan.Minutes > 0)
                return $"{timeSpan.Minutes} minute{(timeSpan.Minutes == 1 ? "" : "s")} ago";
            else
                return "Just now";
        }

        private AcademicEventType MapNotificationTypeToEventType(NotificationType notificationType)
        {
            return notificationType switch
            {
                NotificationType.Assignment => AcademicEventType.Assignment,
                NotificationType.Result => AcademicEventType.Result,
                NotificationType.Notice => AcademicEventType.Notice,
                NotificationType.Deadline => AcademicEventType.Assignment,
                _ => AcademicEventType.Other
            };
        }

        private EventPriority MapNotificationPriorityToEventPriority(NotificationPriority notificationPriority)
        {
            return notificationPriority switch
            {
                NotificationPriority.Low => EventPriority.Low,
                NotificationPriority.Normal => EventPriority.Normal,
                NotificationPriority.High => EventPriority.High,
                NotificationPriority.Critical => EventPriority.Critical,
                _ => EventPriority.Normal
            };
        }

        private string GetCategoryFromEventType(AcademicEventType eventType)
        {
            return eventType switch
            {
                AcademicEventType.Assignment or AcademicEventType.Exam or AcademicEventType.Quiz => "Academic",
                AcademicEventType.Notice => "Administrative",
                AcademicEventType.Holiday => "Holiday",
                AcademicEventType.ClassScheduled or AcademicEventType.ClassCancelled => "Schedule",
                _ => "Other"
            };
        }

        private string GetIconClassForEventType(AcademicEventType eventType)
        {
            return eventType switch
            {
                AcademicEventType.Assignment => "fas fa-tasks",
                AcademicEventType.Exam => "fas fa-graduation-cap",
                AcademicEventType.Quiz => "fas fa-question-circle",
                AcademicEventType.Project => "fas fa-project-diagram",
                AcademicEventType.Notice => "fas fa-bullhorn",
                AcademicEventType.Result => "fas fa-chart-line",
                AcademicEventType.ClassScheduled => "fas fa-chalkboard-teacher",
                AcademicEventType.ClassCancelled => "fas fa-times-circle",
                AcademicEventType.Holiday => "fas fa-flag",
                AcademicEventType.Semester => "fas fa-calendar-alt",
                _ => "fas fa-calendar"
            };
        }

        private string GetColorForEventType(AcademicEventType eventType)
        {
            return eventType switch
            {
                AcademicEventType.Assignment => "#007bff",
                AcademicEventType.Exam => "#dc3545",
                AcademicEventType.Quiz => "#ffc107",
                AcademicEventType.Result => "#28a745",
                AcademicEventType.Notice => "#17a2b8",
                AcademicEventType.Holiday => "#fd7e14",
                AcademicEventType.ClassCancelled => "#6c757d",
                _ => "#6f42c1"
            };
        }        private async Task<int> SyncAssignmentEventsAsync(List<StudentAssignment> assignments)
        {
            int createdCount = 0;
            
            foreach (var assignment in assignments)
            {
                // Check if event already exists in database
                var existingEvents = await _databaseService.GetEventsForUserAsync(assignment.StudentId, "Student");
                var existingEvent = existingEvents.FirstOrDefault(e => 
                    e.SourceType == "Assignment" && e.SourceId == assignment.AssignmentId);
                    
                if (existingEvent == null)
                {
                    var academicEvent = new AcademicEvent
                    {
                        Id = $"assign_{assignment.AssignmentId}",
                        Title = $"{assignment.Title} Due",
                        Description = assignment.Description ?? string.Empty,
                        EventType = AcademicEventType.Assignment,
                        StartDate = assignment.DueDate,
                        CourseId = assignment.CourseId,
                        CourseName = assignment.CourseName,
                        CourseCode = assignment.CourseCode,
                        IsSystemGenerated = true,
                        SourceType = "Assignment",
                        SourceId = assignment.AssignmentId,
                        Color = "#007bff",
                        IconClass = "fas fa-tasks",
                        TargetRole = EventTargetRole.Student,
                        CreatedBy = "system",
                        CreatedByRole = "System",
                        Status = EventStatus.Active,
                        IsPublished = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    // Save to database
                    var eventDb = MapToEventDb(academicEvent);
                    await _databaseService.InsertAsync(eventDb);
                    createdCount++;
                }
            }
            
            return createdCount;
        }        private async Task<int> SyncTeacherAssignmentEventsAsync(List<AssignmentModel> assignments)
        {
            int createdCount = 0;
            
            foreach (var assignment in assignments)
            {
                // Check if event already exists in database for this assignment
                var existingEvents = await _databaseService.GetEventsForUserAsync("system", "Teacher");
                var existingEvent = existingEvents.FirstOrDefault(e => 
                    e.SourceType == "Assignment" && e.SourceId == assignment.AssignmentId);
                    
                if (existingEvent == null)
                {
                    var academicEvent = new AcademicEvent
                    {
                        Id = $"teach_assign_{assignment.AssignmentId}",
                        Title = $"{assignment.Title} Due",
                        Description = assignment.Description ?? string.Empty,
                        EventType = AcademicEventType.Assignment,
                        StartDate = assignment.DueDate,
                        CourseId = assignment.CourseId,
                        CourseName = assignment.CourseName,
                        CourseCode = assignment.CourseCode,
                        IsSystemGenerated = true,
                        SourceType = "Assignment",
                        SourceId = assignment.AssignmentId,
                        Color = "#007bff",
                        IconClass = "fas fa-tasks",
                        TargetRole = EventTargetRole.Teacher,
                        CreatedBy = "system",
                        CreatedByRole = "System",
                        Status = EventStatus.Active,
                        IsPublished = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    // Save to database
                    var eventDb = MapToEventDb(academicEvent);
                    await _databaseService.InsertAsync(eventDb);
                    createdCount++;
                }
            }
            
            return createdCount;
        }        private async Task<int> SyncNotificationEventsAsync(List<NotificationModel> notifications)
        {
            int createdCount = 0;
            
            foreach (var notification in notifications.Where(n => n.Type == NotificationType.Notice))
            {
                // Check if event already exists in database for this notification
                var existingEvents = await _databaseService.GetEventsForUserAsync("system", "All");
                var existingEvent = existingEvents.FirstOrDefault(e => 
                    e.SourceType == "Notification" && e.SourceId == notification.Id);
                    
                if (existingEvent == null)
                {
                    var academicEvent = new AcademicEvent
                    {
                        Id = $"notif_{notification.Id}",
                        Title = notification.Title,
                        Description = notification.Message,
                        EventType = AcademicEventType.Notice,
                        StartDate = notification.CreatedAt,
                        IsAllDay = true,
                        IsSystemGenerated = true,
                        SourceType = "Notification",
                        SourceId = notification.Id,
                        Color = "#17a2b8",
                        IconClass = "fas fa-bullhorn",
                        TargetRole = EventTargetRole.All,
                        CreatedBy = "system",
                        CreatedByRole = "System",
                        Status = EventStatus.Active,
                        IsPublished = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    // Save to database
                    var eventDb = MapToEventDb(academicEvent);
                    await _databaseService.InsertAsync(eventDb);
                    createdCount++;
                }
            }
            
            return createdCount;
        }        /// <summary>
        /// Get calendar events for widgets (simplified format)
        /// </summary>
        public async Task<List<CalendarEvent>> GetEventsAsync(DateTime startDate, DateTime endDate)
        {
            // Get events from database for all roles
            var allEvents = await _databaseService.GetEventsForUserAsync("system", "All");
            
            var academicEvents = allEvents.Where(e => 
                e.StartDate >= startDate && 
                e.StartDate <= endDate &&
                e.Status != EventStatus.Cancelled
            ).ToList();

            return academicEvents.Select(ConvertToCalendarEvent).ToList();
        }        /// <summary>
        /// Create calendar event from widget
        /// </summary>
        public async Task<CalendarEvent?> CreateEventAsync(CalendarEvent calendarEvent, string userId, string userRole)
        {
            try
            {
                var academicEvent = new AcademicEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = calendarEvent.Title,
                    Description = calendarEvent.Description,
                    StartDate = calendarEvent.StartDate,
                    EndDate = calendarEvent.EndDate,
                    IsAllDay = !calendarEvent.HasTime,
                    StartTime = calendarEvent.HasTime ? calendarEvent.StartDate.TimeOfDay : null,
                    EndTime = calendarEvent.HasTime && calendarEvent.EndDate.HasValue ? calendarEvent.EndDate.Value.TimeOfDay : null,
                    Venue = calendarEvent.Location,
                    CourseId = calendarEvent.CourseId,
                    CourseName = calendarEvent.CourseName,
                    CreatedBy = userId,
                    CreatedByName = calendarEvent.CreatedByName,
                    CreatedByRole = userRole,
                    Priority = calendarEvent.Priority,
                    Color = calendarEvent.Color,
                    EventType = ConvertEventType(calendarEvent.Type),
                    TargetRole = EventTargetRole.All,
                    Status = EventStatus.Active,
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Save to database
                var eventDb = MapToEventDb(academicEvent);
                var savedEvent = await _databaseService.InsertAsync(eventDb);
                
                if (savedEvent != null)
                {
                    return ConvertToCalendarEvent(academicEvent);
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating calendar event from widget");
                return null;
            }
        }

        /// <summary>
        /// Convert AcademicEvent to CalendarEvent
        /// </summary>
        private CalendarEvent ConvertToCalendarEvent(AcademicEvent academicEvent)
        {
            return new CalendarEvent
            {
                Id = academicEvent.Id,
                Title = academicEvent.Title,
                Description = academicEvent.Description,
                StartDate = academicEvent.StartDate,
                EndDate = academicEvent.EndDate,
                HasTime = !academicEvent.IsAllDay,
                Location = academicEvent.Venue ?? string.Empty,
                Type = ConvertAcademicEventType(academicEvent.EventType),
                Priority = academicEvent.Priority,
                Color = academicEvent.Color,
                CourseId = academicEvent.CourseId ?? string.Empty,
                CourseName = academicEvent.CourseName ?? string.Empty,
                CreatedBy = academicEvent.CreatedBy,
                CreatedByName = academicEvent.CreatedByName
            };
        }        /// <summary>
        /// Convert EventType to AcademicEventType
        /// </summary>
        private AcademicEventType ConvertEventType(EventType eventType)
        {
            return eventType switch
            {
                EventType.Assignment => AcademicEventType.Assignment,
                EventType.Exam => AcademicEventType.Exam,
                EventType.Class => AcademicEventType.ClassScheduled,
                EventType.Event => AcademicEventType.Other,
                EventType.Holiday => AcademicEventType.Holiday,
                EventType.Notice => AcademicEventType.Notice,
                EventType.Meeting => AcademicEventType.Meeting,
                EventType.Deadline => AcademicEventType.Assignment,
                _ => AcademicEventType.Other
            };
        }

        /// <summary>
        /// Convert AcademicEventType to EventType
        /// </summary>
        private EventType ConvertAcademicEventType(AcademicEventType academicEventType)
        {
            return academicEventType switch
            {
                AcademicEventType.Assignment => EventType.Assignment,
                AcademicEventType.Exam => EventType.Exam,
                AcademicEventType.ClassScheduled => EventType.Class,
                AcademicEventType.Other => EventType.Event,
                AcademicEventType.Holiday => EventType.Holiday,
                AcademicEventType.Notice => EventType.Notice,
                AcademicEventType.Meeting => EventType.Meeting,
                _ => EventType.Event
            };
        }

        /// <summary>
        /// Synchronizes calendar with automation engine events
        /// </summary>
        public async Task SynchronizeWithAutomationAsync()
        {
            try
            {
                // Sync assignments
                await SyncAssignmentsAsync();
                
                // Sync notifications
                await SyncNotificationsAsync();
                
                // Future: Sync exam schedules, class schedules, etc.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error synchronizing with automation: {ex.Message}");
            }
        }        private async Task SyncAssignmentsAsync()
        {
            // Implementation for syncing assignments
            await Task.CompletedTask;
        }

        private async Task SyncNotificationsAsync()
        {
            // Implementation for syncing notifications
            await Task.CompletedTask;
        }        private AcademicEventDb MapToEventDb(AcademicEvent academicEvent)
        {            return new AcademicEventDb
            {
                Id = academicEvent.Id,
                Title = academicEvent.Title,
                Description = academicEvent.Description,
                EventType = academicEvent.EventType.ToString().ToLower(),
                Category = academicEvent.Category,
                Priority = academicEvent.Priority.ToString().ToLower(),
                StartDate = academicEvent.StartDate,
                EndDate = academicEvent.EndDate,
                StartTime = academicEvent.StartTime,
                EndTime = academicEvent.EndTime,
                IsAllDay = academicEvent.IsAllDay,
                Venue = academicEvent.Venue,
                Room = academicEvent.Room,
                Building = academicEvent.Building,
                OnlineLink = academicEvent.OnlineLink,
                CourseId = academicEvent.CourseId,
                DepartmentId = academicEvent.DepartmentId,
                TargetRole = academicEvent.TargetRole.ToString().ToLower(),
                TargetUserIds = academicEvent.TargetUserIds,
                TargetCourseIds = academicEvent.TargetCourseIds,                TargetDepartmentIds = academicEvent.TargetDepartmentIds,
                CreatedBy = academicEvent.CreatedBy,
                CreatedByRole = academicEvent.CreatedByRole,
                Status = academicEvent.Status.ToString().ToLower(),
                IsPublished = academicEvent.IsPublished,
                IsRecurring = academicEvent.IsRecurring,
                RecurringPattern = ConvertRecurringPatternToDict(academicEvent.RecurringPattern),
                IsSystemGenerated = academicEvent.IsSystemGenerated,
                SourceType = academicEvent.SourceType,
                SourceId = academicEvent.SourceId,
                AutomationRuleId = academicEvent.AutomationRuleId,
                HasReminder = academicEvent.HasReminder,
                Color = academicEvent.Color,
                IconClass = academicEvent.IconClass,
                BadgeClass = academicEvent.BadgeClass,
                Tags = academicEvent.Tags,
                CreatedAt = academicEvent.CreatedAt,
                UpdatedAt = academicEvent.UpdatedAt ?? DateTime.UtcNow
            };
        }

        private Dictionary<string, object>? ConvertRecurringPatternToDict(RecurringPattern? pattern)
        {
            if (pattern == null) return null;
            
            return new Dictionary<string, object>
            {
                ["type"] = pattern.Type.ToString(),
                ["interval"] = pattern.Interval,
                ["endDate"] = pattern.EndDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                ["maxOccurrences"] = pattern.MaxOccurrences ?? 0,
                ["daysOfWeek"] = pattern.DaysOfWeek.Select(d => (int)d).ToList()
            };
        }
    }

    /// <summary>
    /// Result of bulk import operation
    /// </summary>
    public class BulkImportResult
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> SuccessfulEvents { get; set; } = new();
        public List<BulkImportError> FailedEvents { get; set; } = new();
    }/// <summary>
    /// Error information for failed bulk import items
    /// </summary>
    public class BulkImportError
    {
        public string EventTitle { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}

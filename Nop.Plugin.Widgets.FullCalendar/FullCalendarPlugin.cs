﻿using System.Collections.Generic;
using System.IO;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Plugin.Widgets.FullCalendar.Data;
using Nop.Services.Messages;
using Nop.Core.Domain.Messages;
using Nop.Services.Plugins;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.FullCalendar
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class FullCalendarPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly FullCalendarObjectContext _objectContext;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IStoreContext _storeContext;

        public FullCalendarPlugin(IPictureService pictureService,
            ISettingService settingService, IWebHelper webHelper,
            FullCalendarObjectContext objectContext,
            IMessageTemplateService messageTemplateService,
            IStoreContext storeContext)
        {
            this._pictureService = pictureService;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._objectContext = objectContext;
            this._messageTemplateService = messageTemplateService;
            this._storeContext = storeContext;
        }


        /// <summary>
        /// Install plugin
        /// </summary>
        public override async Task InstallAsync()
        {
            //database objects
            _objectContext.Install();

            //create appointment message template
            var messageTemplate = new MessageTemplate
            {
                Name = "Appointment.New",
                Subject = "%Store.Name%: New appointment requested for %Customer.FullName%.",
                Body = "<p><a href=\"%Store.URL%/Admin/Customer/Edit/%Customer.Id%\">%Customer.FullName%</a> has requested a new appointment.</p><dl><dt>Contact Number:</dt><dd>%Appointment.ContactNumber%</dd><dt>Email:</dt><dd>%Customer.Email%</dd><dt>Date:</dt><dd>%Appointment.Date%</dd><dt>Reason:</dt><dd>%Appointment.Reason%</dd></dl>",
                IsActive = true,
                EmailAccountId = 1,
            };
            await _messageTemplateService.InsertMessageTemplateAsync(messageTemplate);

            //settings
            var settings = new FullCalendarSettings
            {
                CalendarId = "38tvhnke4vtaj8u6or8o59out4@group.calendar.google.com",
                PublicApiKey = "AIzaSyB-oGNxrlENnrcBpGtizNOCaoGgH9q4h6Q",
                ClassName = "cal3",
            };

            await _settingService.SaveSettingAsync(settings);

            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Instructions", "<h5>Make your Google Calendar public:</h5><ol><li>In the Google Calendar interface, locate the \"My calendars\" area on the left.</li><div>Hover over the calendar you need and click the downward arrow.</li><li>A menu will appear. Click \"Share this Calendar\".</li><li>Check \"Make this calendar public\".</li><li>Make sure \"Share only my free/busy information\" is unchecked.</li><li>Click \"Save\".</li></ol><h5>Obtain your Google Calendar's ID:</h5><ol><li>In the Google Calendar interface, locate the \"My calendars\" area on the left.</li><li>Hover over the calendar you need and click the downward arrow.</li><li>A menu will appear. Click \"Calendar settings\".</li><li>In the \"Calendar Address\" section of the screen, you will see your Calendar ID. It will look something like \"abcd1234@group.calendar.google.com\".</li></ol>");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.Success", "Requested Appointment");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.Error", "Failing adding appointment, please contact your vet.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.AppointmentDate", "Appointment Date");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.AppointmentReason", "Appointment Reason");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.AppointmentReasonReq", "THIS CANNOT BE NULL");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.AppointmentDateReq", "THIS CANN OT BE NULL");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.ContactName", "Contact Name");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.ContactNumber", "Contact Number");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.ContactNumberRequired", "Contact Number is required");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.PhoneHelperText", "Enter a number where we can contact you.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.Title", "Request an Appointment");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.CalendarId", "Google Calendar Id");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.ClassName", "Class Name");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.PublicApiKey", "Public Api Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.CreatedOn", "Created On");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.DaysOfWeekEnabled", "Days Of Week Enabled");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.StartTime", "Start Time");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.EndTime", "End Time");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.Google.Header", "Google Calendar Settings");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.DaysOfWeek.Header", "Days of Operation");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.DaysOfWeek.Instructions", "Check which days your store is open.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.Hours.Header", "Hours of Operation");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.Hours.Instructions", "Set which hours your store is open.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.Title", "Calendar");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.FullCalendar.BodyText", "Change body text");

            await base.InstallAsync();

        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            _objectContext.Uninstall();

            var store = _storeContext.CurrentStore;
            var messageTemplate = _messageTemplateService.GetMessageTemplateByName("Appointment.New", store.Id);
            if (messageTemplate != null)
                _messageTemplateService.DeleteMessageTemplate(messageTemplate);
            
            //settings
            _settingService.DeleteSetting<FullCalendarSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Instructions");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.Success");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.Error");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.AppointmentDate");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.AppointmentReason");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.AppointmentReasonReq");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.AppointmentDateReq");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.ContactName");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.ContactNumber");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.ContactNumberRequired");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.PhoneHelperText");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Form.Title");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.CalendarId");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.ClassName");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.PublicApiKey");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.CreatedOn");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.DaysOfWeekEnabled");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.StartTime");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.EndTime");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.Google.Header");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.DaysOfWeek.Header");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.DaysOfWeek.Instructions");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.Hours.Header");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Config.Hours.Instructions");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.Title");
            this.DeletePluginLocaleResource("Plugins.Widgets.FullCalendar.BodyText");

            await base.UninstallAsync();
        }
        public bool HideInWidgetList => false;

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.HomepageBeforeNews });

        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "WidgetsFullCalendar";
        }
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/WidgetsFullCalendar/Configure";
        }
    }
}

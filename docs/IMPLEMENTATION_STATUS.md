# Implementation status

## MVP-0.1: Windows notification proof of concept

- [x] Packaged WinUI 3 project scaffold
- [x] `userNotificationListener` capability (`uap3:Capability`)
- [x] Permission request from UI
- [x] Read current toast notifications
- [x] Parse title/body/raw text elements
- [x] Edge / Instagram source-app candidate filter
- [x] Foreground notification-change event subscription
- [x] Notification ID diffing
- [x] JSONL local append-only log
- [x] Local log deletion control
- [ ] Build and run on Windows (requires Windows + .NET/Windows App SDK workload)
- [ ] Verify actual Edge + Instagram notification payload
- [ ] Add DM-specific parser after observing real notification formats

## Known limitation

The listener exposes app-level notification metadata. It does not guarantee that a browser toast can be attributed to a particular website. The dedicated Edge profile is an operational way to reduce unrelated Edge site notifications, not a formal site-origin guarantee.

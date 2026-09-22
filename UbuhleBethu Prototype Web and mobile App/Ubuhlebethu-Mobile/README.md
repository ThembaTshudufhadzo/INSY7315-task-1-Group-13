# UbuhlebethuMApp

An Android prototype for Ubuhlebethu, a construction/site-management company. The app gives four different roles their own view of the same workflow — from a field worker clocking in on site, to a site manager logging materials and expenses, to office/QS staff pricing quotations, to a system admin assigning users.

## Features

The app is role-based: after logging in, the bottom navigation and home screen adapt to the selected role.

- **Login** — role picker (Field Worker, Site Manager, Office/QS Staff, System Admin) that auto-fills demo credentials for the selected role.
- **Field Worker** — start/finish a shift, with check-in and check-out times recorded and shown in an attendance list pending manager approval.
- **Site Manager** — scan/log materials, log work tasks, update site progress, upload a GPS-tagged progress photo, view the team's attendance, view logged expenses, and ask a (simulated) Gemini assistant for a cost estimate based on scanned materials.
- **Office / QS Staff** — enter a project scope and get a (simulated) Gemini-generated quotation suggestion, approve or reject quotations, and generate invoices.
- **System Admin** — assign new users and issue credentials.
- Role-based bottom navigation: each role only sees the sections relevant to it, and Field Workers navigate through a single screen with no bottom nav.
- Logout from any role, returning to the login screen.

> Note: this is a UI prototype. Data (attendance, expenses, quotations, "Gemini" responses) is generated in-memory or hardcoded for demonstration — there's no backend, database, or real AI integration yet.

## Tech Stack

- **Language:** Kotlin
- **UI:** Android Views (XML layouts), Fragments, Material Components, RecyclerView
- **Build system:** Gradle (Kotlin DSL), Gradle version catalog (`libs.versions.toml`)
- **Min SDK:** 25 · **Target SDK:** 35 · **Compile SDK:** 37
- **Java/Kotlin compatibility:** JVM 17

## Project Structure

```
app/src/main/java/com/example/ubuhlebethumapp/
├── LoginActivity.kt          # Role selection + login
├── MainActivity.kt           # Hosts fragments, role-based bottom nav, logout
├── model/
│   ├── UserRole.kt           # Field Worker / Site Manager / Office QS / System Admin
│   ├── AttendanceRecord.kt
│   ├── AttendanceRepository.kt
│   ├── ExpenseItem.kt
│   └── ProjectEstimate.kt
└── ui/
    ├── FieldWorkerFragment.kt
    ├── SiteManagerFragment.kt
    ├── OfficeQSFragment.kt
    ├── ManagementFragment.kt
    ├── AttendanceAdapter.kt
    └── ExpensesAdapter.kt
```

## Getting Started

### Prerequisites

- Android Studio (recent stable release)
- JDK 17
- An Android emulator or physical device running Android 7.1 (API 25) or later

### Setup

1. Open the project folder in Android Studio (`File > Open`, select the folder containing `settings.gradle.kts`).
2. Let Gradle sync and download dependencies.
3. Run the `app` configuration on an emulator or connected device.

### Demo logins

Selecting a role on the login screen auto-fills matching demo credentials:

| Role | Email | Password |
|---|---|---|
| Field Worker | worker@ubuhlebethu.co.za | FieldPass2026 |
| Site Manager | sitemanager@ubuhlebethu.co.za | SitePass2026 |
| Office / QS Staff | qs@ubuhlebethu.co.za | QSPass2026 |
| System Admin | admin@ubuhlebethu.co.za | AdminPass2026 |

Any non-empty email/password combination will log in — credentials aren't validated against a backend.

## Roadmap Ideas

- Replace in-memory/hardcoded data with a real backend (e.g. connect to the companion [UbuhleBethu web app](#) or a REST API).
- Replace the simulated "Gemini" responses with a real AI integration.
- Add persistent authentication and real credential validation.
- Add camera integration for the site manager's progress photo upload.

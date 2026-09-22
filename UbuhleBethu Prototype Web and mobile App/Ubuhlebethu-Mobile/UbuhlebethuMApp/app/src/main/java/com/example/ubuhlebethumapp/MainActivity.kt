package com.example.ubuhlebethumapp

import android.os.Bundle
import android.view.View
import androidx.appcompat.app.AppCompatActivity
import androidx.fragment.app.Fragment
import com.example.ubuhlebethumapp.model.UserRole
import com.example.ubuhlebethumapp.ui.FieldWorkerFragment
import com.example.ubuhlebethumapp.ui.ManagementFragment
import com.example.ubuhlebethumapp.ui.OfficeQSFragment
import com.example.ubuhlebethumapp.ui.SiteManagerFragment
import com.google.android.material.bottomnavigation.BottomNavigationView

class MainActivity : AppCompatActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        val bottomNav = findViewById<BottomNavigationView>(R.id.bottomNavigation)

        val roleName = intent.getStringExtra("USER_ROLE") ?: UserRole.FIELD_WORKER.name
        val userRole = UserRole.valueOf(roleName)

        setupRoleBasedNavigation(bottomNav, userRole)

        when (userRole) {
            UserRole.FIELD_WORKER -> loadFragment(FieldWorkerFragment())
            UserRole.SITE_MANAGER -> loadFragment(SiteManagerFragment())
            UserRole.OFFICE_QS -> loadFragment(OfficeQSFragment())
            UserRole.SYSTEM_ADMIN -> loadFragment(ManagementFragment())
        }

        bottomNav.setOnItemSelectedListener { item ->
            when (item.itemId) {
                R.id.nav_field_worker -> loadFragment(FieldWorkerFragment())
                R.id.nav_site_manager -> loadFragment(SiteManagerFragment())
                R.id.nav_office_qs -> loadFragment(OfficeQSFragment())
                R.id.nav_admin -> loadFragment(ManagementFragment())
                else -> false
            }
        }
    }

    private fun setupRoleBasedNavigation(nav: BottomNavigationView, role: UserRole) {
        val menu = nav.menu

        when (role) {
            UserRole.FIELD_WORKER -> {
                // Field Workers only see the Field Work page; hide bottom nav bar to restrict navigation
                nav.visibility = View.GONE
            }
            UserRole.SITE_MANAGER -> {
                nav.visibility = View.VISIBLE
                menu.findItem(R.id.nav_field_worker).isVisible = true
                menu.findItem(R.id.nav_site_manager).isVisible = true
                menu.findItem(R.id.nav_office_qs).isVisible = false
                menu.findItem(R.id.nav_admin).isVisible = false
            }
            UserRole.OFFICE_QS -> {
                nav.visibility = View.VISIBLE
                menu.findItem(R.id.nav_field_worker).isVisible = true
                menu.findItem(R.id.nav_site_manager).isVisible = true
                menu.findItem(R.id.nav_office_qs).isVisible = true
                menu.findItem(R.id.nav_admin).isVisible = false
            }
            UserRole.SYSTEM_ADMIN -> {
                nav.visibility = View.VISIBLE
                menu.findItem(R.id.nav_field_worker).isVisible = false
                menu.findItem(R.id.nav_site_manager).isVisible = false
                menu.findItem(R.id.nav_office_qs).isVisible = false
                menu.findItem(R.id.nav_admin).isVisible = true
            }
        }
    }

    private fun loadFragment(fragment: Fragment): Boolean {
        supportFragmentManager.beginTransaction()
            .replace(R.id.fragmentContainer, fragment)
            .commit()
        return true
    }

    fun performLogout() {
        val intent = android.content.Intent(this, LoginActivity::class.java)
        intent.flags = android.content.Intent.FLAG_ACTIVITY_NEW_TASK or android.content.Intent.FLAG_ACTIVITY_CLEAR_TASK
        startActivity(intent)
        finish()
    }
}
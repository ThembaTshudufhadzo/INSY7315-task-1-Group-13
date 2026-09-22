package com.example.ubuhlebethumapp

import android.content.Intent
import android.os.Bundle
import android.view.View
import android.widget.AdapterView
import android.widget.ArrayAdapter
import android.widget.Button
import android.widget.Spinner
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.example.ubuhlebethumapp.model.UserRole
import com.google.android.material.textfield.TextInputEditText

class LoginActivity : AppCompatActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_login)

        val spRoleSelect = findViewById<Spinner>(R.id.spRoleSelect)
        val btnLogin = findViewById<Button>(R.id.btnLogin)
        val etEmail = findViewById<TextInputEditText>(R.id.etEmail)
        val etPassword = findViewById<TextInputEditText>(R.id.etPassword)

        val roles = UserRole.values().map { it.displayName }
        val adapter = ArrayAdapter(this, android.R.layout.simple_spinner_dropdown_item, roles)
        spRoleSelect.adapter = adapter

        spRoleSelect.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
            override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long) {
                val selectedRole = UserRole.values()[position]
                when (selectedRole) {
                    UserRole.FIELD_WORKER -> {
                        etEmail.setText("worker@ubuhlebethu.co.za")
                        etPassword.setText("FieldPass2026")
                    }
                    UserRole.SITE_MANAGER -> {
                        etEmail.setText("sitemanager@ubuhlebethu.co.za")
                        etPassword.setText("SitePass2026")
                    }
                    UserRole.OFFICE_QS -> {
                        etEmail.setText("qs@ubuhlebethu.co.za")
                        etPassword.setText("QSPass2026")
                    }
                    UserRole.SYSTEM_ADMIN -> {
                        etEmail.setText("admin@ubuhlebethu.co.za")
                        etPassword.setText("AdminPass2026")
                    }
                }
            }

            override fun onNothingSelected(parent: AdapterView<*>?) {}
        }

        btnLogin.setOnClickListener {
            val selectedRoleIndex = spRoleSelect.selectedItemPosition
            val selectedRole = UserRole.values()[selectedRoleIndex]
            val email = etEmail.text.toString().trim()
            val password = etPassword.text.toString().trim()

            if (email.isEmpty() || password.isEmpty()) {
                Toast.makeText(this, "Please enter your email and password", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }

            Toast.makeText(this, "Success: Logged in as ${selectedRole.displayName}", Toast.LENGTH_SHORT).show()

            val intent = Intent(this, MainActivity::class.java).apply {
                putExtra("USER_ROLE", selectedRole.name)
                putExtra("USER_EMAIL", email)
            }
            startActivity(intent)
            finish()
        }
    }
}
package com.example.ubuhlebethumapp.model

class ExpenseItem (
    val managerName: String,
    val itemDescription: String,
    val amount: Double,
    var status: String = "PENDING" // "APPROVED", "REJECTED", or "PENDING"
)
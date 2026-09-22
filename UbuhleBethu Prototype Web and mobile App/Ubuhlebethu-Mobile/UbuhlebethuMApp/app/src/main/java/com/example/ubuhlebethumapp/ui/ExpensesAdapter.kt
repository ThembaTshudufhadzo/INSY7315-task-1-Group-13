package com.example.ubuhlebethumapp.ui

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.example.ubuhlebethumapp.R
import com.example.ubuhlebethumapp.model.ExpenseItem

class ExpensesAdapter(private val list: List<ExpenseItem>) :
    RecyclerView.Adapter<ExpensesAdapter.ViewHolder>() {

    class ViewHolder(view: View) : RecyclerView.ViewHolder(view) {
        val title: TextView = view.findViewById(R.id.tvExpenseTitle)
        val amount: TextView = view.findViewById(R.id.tvExpenseAmount)
        val approveBtn: Button = view.findViewById(R.id.btnApprove)
        val rejectBtn: Button = view.findViewById(R.id.btnReject)
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val view = LayoutInflater.from(parent.context).inflate(R.layout.item_expense, parent, false)
        return ViewHolder(view)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        val item = list[position]
        holder.title.text = "${item.managerName} > ${item.itemDescription}"
        holder.amount.text = "R ${String.format("%.2f", item.amount)}"

        holder.approveBtn.setOnClickListener {
            item.status = "APPROVED"
            holder.approveBtn.text = "APPROVED"
            holder.rejectBtn.visibility = View.GONE
        }

        holder.rejectBtn.setOnClickListener {
            item.status = "REJECTED"
            holder.rejectBtn.text = "REJECTED"
            holder.approveBtn.visibility = View.GONE
        }
    }

    override fun getItemCount(): Int = list.size
}
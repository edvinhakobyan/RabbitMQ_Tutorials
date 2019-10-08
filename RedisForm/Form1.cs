using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MetroFramework.Forms;
using MetroFramework;
using ServiceStack.Redis;
using ServiceStack.Redis.Generic;

namespace RedisForm
{
    public partial class Form1 : MetroForm
    {
        public Form1()
        {
            InitializeComponent();
        }

        void Edit(bool value)
        {
            TextBoxID.ReadOnly = value;
            TextBoxManufacturer.ReadOnly = value;
            TextBoxModel.ReadOnly = value;
        }

        void ClearText()
        {
            TextBoxID.Text = string.Empty;
            TextBoxManufacturer.Text = string.Empty;
            TextBoxModel.Text = string.Empty;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using (RedisClient client = new RedisClient("localhost", 6379))
            {
                IRedisTypedClient<Phone> phone = client.As<Phone>();
                phoneBindingSource.DataSource = phone.GetAll();
                phone.
                Edit(true);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            phoneBindingSource.Add(new Phone());
            phoneBindingSource.MoveLast();
            Edit(false);
            TextBoxID.Focus();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            Edit(false);
            TextBoxID.Focus();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Edit(true);
            phoneBindingSource.ResetBindings(false);
            ClearText();

        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (MetroMessageBox.Show(this, "Are you sure want to delete this record ?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Phone p = phoneBindingSource.Current as Phone;
                if (p != null)
                {
                    using (RedisClient client = new RedisClient("localhost", 6379))
                    {
                        IRedisTypedClient<Phone> phone = client.As<Phone>();
                        phone.DeleteById(p.ID);
                        phoneBindingSource.RemoveCurrent();
                        ClearText();
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            using (RedisClient client = new RedisClient("localhost", 6379))
            {
                phoneBindingSource.EndEdit();
                IRedisTypedClient<Phone> phone = client.As<Phone>();
                phone.StoreAll(phoneBindingSource.DataSource as List<Phone>);
                MetroMessageBox.Show(this, "Your data has been successfully saved.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearText();
                Edit(true);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace FIrebase
{
    

    public partial class FIrebase : Form
    {
        IFirebaseConfig config = new FirebaseConfig()
        {
            AuthSecret = "GlkCAeZHNmv454HVDnEy94mAhiSTLujs5moC1HVk",
            BasePath = @"https://fir-ae5f4.firebaseio.com/"
        };

        IFirebaseClient client;

        public FIrebase()
        {
            InitializeComponent();
        }

        private void FIrebase_Load(object sender, EventArgs e)
        {
            client = new FirebaseClient(config);

            if (client != null)
                MessageBox.Show("Connection is established");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var data = new Data()
            {
                Id = textBox1.Text,
                Adress = textBox2.Text,
                Name = textBox3.Text,
                Age = textBox4.Text,
            };


            SetResponse response = await client.SetAsync("Information/" + textBox1.Text, data);
            Data result = response.ResultAs<Data>();

            MessageBox.Show("Data Inserted " + result.Id);


        }

        private async void button2_Click(object sender, EventArgs e)
        {
            FirebaseResponse response = await client.GetAsync("Information/" + textBox1.Text);
            Data obj = response.ResultAs<Data>();


            try
            {
                textBox1.Text = obj.Id;
                textBox2.Text = obj.Adress;
                textBox3.Text = obj.Name;
                textBox4.Text = obj.Age;
                MessageBox.Show("Data Retrive Sacsesfuly ");
            }
            catch
            {
                MessageBox.Show("Data Error");
            }


        }

        private async void button3_Click(object sender, EventArgs e)
        {
            var data = new Data()
            {
                Id = textBox1.Text,
                Adress = textBox2.Text,
                Name = textBox3.Text,
                Age = textBox4.Text,
            };

            FirebaseResponse response = await client.UpdateAsync("Information/" + textBox1.Text, data);
            Data obj = response.ResultAs<Data>();

            MessageBox.Show("Data Update...");

        }

        private async void button4_Click(object sender, EventArgs e)
        {
            FirebaseResponse response = await client.DeleteAsync("Information/" + textBox1.Text);
            MessageBox.Show("Data Deleted...");

        }

        private async void button5_Click(object sender, EventArgs e)
        {
            FirebaseResponse response = await client.DeleteAsync("Information/");
            MessageBox.Show("All Data Deleted...");
        }
    }
}

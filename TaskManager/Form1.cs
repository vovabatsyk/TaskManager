using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace TaskManager
{
    public partial class TaskManager : Form
    {
        private List<Process> _processes = null;
        private ListViewItemComparer ItemComparer = null;

        public TaskManager()
        {
            InitializeComponent();
        }

        #region Events
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void TaskManager_Load(object sender, EventArgs e)
        {
            _processes = new List<Process>();
            GetProcesses();
            RefreshProcessesList();

            ItemComparer = new ListViewItemComparer();
            ItemComparer.ColumnIndex = 0;
        }


        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            GetProcesses();
            RefreshProcessesList();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
                Kill();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            KillTree();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Kill();
        }

        private void stopRangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KillTree();
        }

        private void runNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Interaction.InputBox("Enter new task", "Run New");

            try
            {
                Process.Start(path);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            GetProcesses();

            List<Process> filteredProcesses = _processes.Where(x =>
                x.ProcessName.ToLower().Contains(toolStripTextBox1.Text.ToLower())).ToList();

            RefreshProcessesList(filteredProcesses);
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ItemComparer.ColumnIndex = e.Column;
            ItemComparer.SortOrder = ItemComparer.SortOrder == SortOrder.Ascending
                ? SortOrder.Descending
                : SortOrder.Ascending;

            listView1.ListViewItemSorter = ItemComparer;

            listView1.Sort();
        }

        #endregion


        #region My Methods

        private void GetProcesses()
        {
            _processes.Clear();

            _processes = Process.GetProcesses().ToList<Process>();
        }

        private void RefreshProcessesList()
        {
            listView1.Items.Clear();

            double memorySize = 0;

            foreach (var process in _processes)
            {
                memorySize = 0;

                PerformanceCounter pc = new PerformanceCounter();
                pc.CategoryName = "Process";
                pc.CounterName = "Working Set - Private";
                pc.InstanceName = process.ProcessName;

                memorySize = (double)pc.NextValue() / (1000 * 1000);

                string[] row = new string[]
                {
                    process.ProcessName.ToString(),
                    process.Id.ToString(),
                    process.BasePriority.ToString(),
                    process.Threads.Count.ToString(),
                    Math.Round(memorySize, 1).ToString()
                };

                listView1.Items.Add(new ListViewItem(row));

                pc.Close();
                pc.Dispose();
            }

            toolStripStatusLabel1.Text = $@"Running {_processes.Count.ToString()} processes";
        }

        private void RefreshProcessesList(List<Process> processes)
        {
            try
            {


                listView1.Items.Clear();

                double memorySize = 0;

                foreach (var process in processes)
                {
                    if (process != null)
                    {
                        memorySize = 0;

                        PerformanceCounter pc = new PerformanceCounter();
                        pc.CategoryName = "Process";
                        pc.CounterName = "Working Set - Private";
                        pc.InstanceName = process.ProcessName;

                        memorySize = (double)pc.NextValue() / (1000 * 1000);

                        string[] row = new string[]
                        {
                        process.ProcessName.ToString(),
                        process.Id.ToString(),
                        process.BasePriority.ToString(),
                        process.Threads.Count.ToString(),
                        Math.Round(memorySize, 1).ToString()
                        };

                        listView1.Items.Add(new ListViewItem(row));

                        pc.Close();
                        pc.Dispose();
                    }
                }

                toolStripStatusLabel1.Text = $@"Running {processes.Count.ToString()} processes";
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void KillProcess(Process process)
        {
            process.Kill();
            process.WaitForExit();
        }

        private void KillProcessAndChildren(int processId)
        {
            if (processId == 0) return;

            var searcher = new ManagementObjectSearcher(
            "Select * From Win32_Process Where ParentProcessID=" + processId
                );

            var objectCollection = searcher.Get();

            foreach (var obj in objectCollection)
            {
                KillProcessAndChildren(Convert.ToInt32(obj["ProcessID"]));
            }

            try
            {
                Process process = Process.GetProcessById(processId);
                process.Kill();
                process.WaitForExit();
            }
            catch (ArgumentException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetParentProcessId(Process process)
        {
            int parentId = 0;

            try
            {
                ManagementObject managementObject = new ManagementObject("win32_process.handle='" + process.Id + "'");
                managementObject.Get();
                parentId = Convert.ToInt32(managementObject["ParentProcessId"]);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return parentId;
        }

        private void Kill()
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process process = _processes.Where(x => x.ProcessName == listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcess(process);
                    GetProcesses();
                    RefreshProcessesList();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void KillTree()
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process process = _processes.Where(x => x.ProcessName == listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcessAndChildren(GetParentProcessId(process));

                    GetProcesses();
                    RefreshProcessesList();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

    }
}

// Curtin University
// Mechatronics Engineering
// Bluetooth I/O Card - Sample GUI Code

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BluetoothGUISample
{    

    public partial class Form1 : Form
    {
        // Declare variables to store inputs and outputs.
        bool runBluetooth = true;
        bool byteRead = false;
        int Input1 = 0;
        int Input2 = 0;

        byte[] Outputs = new byte[4];
        byte[] Inputs = new byte[4];

        const byte START = 255;
        const byte ZERO = 0;

        public Form1()
        {
            // Initialize required for form controls.
            InitializeComponent();

            // Establish connection with Bluetooth IOCard
            if (runBluetooth == true)
            {
                if (!bluetooth.IsOpen)                                  // Check if the bluetooth has been connected.
                {
                    try
                    {
                        bluetooth.Open();                               //Try to connect to the bluetooth.
                    }
                    catch
                    {
                        statusBox.Enabled = false;
                        statusBox.Text ="ERROR: Failed to connect.";     //If the bluetooth does not connect return an error.
                    }
                }
            }
        }

        // Send a four byte message to the Arduino via serial.
        private void sendIO(byte PORT, byte DATA)
        {
            Outputs[0] = START;    //Set the first byte to the start value that indicates the beginning of the message.
            Outputs[1] = PORT;     //Set the second byte to represent the port where, Input 1 = 0, Input 2 = 1, Output 1 = 2 & Output 2 = 3. This could be enumerated to make writing code simpler... (see Arduino driver)
            Outputs[2] = DATA;  //Set the third byte to the value to be assigned to the port. This is only necessary for outputs, however it is best to assign a consistent value such as 0 for input ports.
            Outputs[3] = (byte)(START + PORT + DATA); //Calculate the checksum byte, the same calculation is performed on the Arduino side to confirm the message was received correctly.

            if (bluetooth.IsOpen)
            {
                bluetooth.Write(Outputs, 0, 4);         //Send all four bytes to the IO card.                      
            }
        }

        private void Send1_Click(object sender, EventArgs e) //Press the button to send the value to Output 1, Arduino Port A.
        {
            decimal volt_plus = OutputBox1.Value + (decimal)15; //Converts the range from 0 to 30V, making it easier to implement the equation.                                                                                                                                                                                                  
            int value = (int) ((volt_plus*255)/((decimal) 30 ) + (decimal) 1); //Equation to calculate the bits to send to Ardunio
            if (value > 255) //If a value is greater than the max, set it to the max.
                value = 255;
            InputBox1.Text = value.ToString(); //Send the bit value to the InputBox on GUI

            sendIO(2, (byte)value); // The value 2 indicates Output1, value for output set in OutputBox1.
        }

        private void Send2_Click(object sender, EventArgs e) //Press the button to send the value to Output 2, Arduino Port C.
        {
            sendIO(3, (byte)OutputBox2.Value); // The value 3 indicates Output2, value for output set in OutputBox1.
        }

        private void Get1_Click(object sender, EventArgs e) //Press the button to request value from Input 1, Arduino Port F.
        {
            sendIO(0, ZERO);  // The value 0 indicates Input 1, ZERO just maintains a fixed value for the discarded data in order to maintain a consistent package format.
        }

        private void Get2_Click(object sender, EventArgs e) //Press the button to request value from Input 1, Arduino Port K.
        {
            sendIO(1, ZERO);  // The value 1 indicates Input 2, ZERO maintains a consistent value for the message output.
        }

        private void getIOtimer_Tick(object sender, EventArgs e) //It is best to continuously check for incoming data as handling the buffer or waiting for event is not practical in C#.
        {
            if (bluetooth.IsOpen) //Check that a serial connection exists.
            {
                if (bluetooth.BytesToRead >= 4) //Check that the buffer contains a full four byte package.
                {
                    //statusBox.Text = "Incoming"; // A status box can be used for debugging code.
                    Inputs[0] = (byte)bluetooth.ReadByte(); //Read the first byte of the package.

                    if (Inputs[0] == START) //Check that the first byte is in fact the start byte.
                    {
                        //statusBox.Text = "Start Accepted";

                        //Read the rest of the package.
                        Inputs[1] = (byte)bluetooth.ReadByte();
                        Inputs[2] = (byte)bluetooth.ReadByte();
                        Inputs[3] = (byte)bluetooth.ReadByte();

                        //Calculate the checksum.
                        byte checkSum = (byte)(Inputs[0] + Inputs[1] + Inputs[2]);

                        //Check that the calculated check sum matches the checksum sent with the message.
                        if (Inputs[3] == checkSum)
                        {
                            //statusBox.Text = "CheckSum Accepted";

                            //Check which port the incoming data is associated with.
                            switch (Inputs[1])
                            {
                                case 0: //Save the data to a variable and place in the textbox.
                                    //statusBox.Text = "Input1";
                                    Input1 = Inputs[2];
                                    InputBox1.Text = Input1.ToString();
                                    break;
                                case 1: //Save the data to a variable and place in the textbox. 
                                    //statusBox.Text = "Input2";
                                    Input2 = Inputs[2];
                                    InputBox2.Text = Input2.ToString();
                                    break;
                            }
                        }
                    }
                }
            }
        }

    }
    
}

﻿//---------------------------------------------------------------------------------
// Copyright (c) July 2021, devMobile Software
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//---------------------------------------------------------------------------------
using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace devMobile.IoT.SX127x.ReceiveBasic
{
	public sealed class SX127XDevice
	{
		private const byte RegisterAddressMinimum = 0X0;
		private const byte RegisterAddressMaximum = 0x42;
		private const byte RegisterAddressReadMask = 0X7f;
		private const byte RegisterAddressWriteMask = 0x80;

		private readonly int SpiBusId;
		private readonly int ChipSelectLogicalPinNumber;
		private readonly SpiDevice SX127XTransceiver = null;
		private readonly GpioController gpioController = null;

		public SX127XDevice(int spiBusId = 0, int chipSelectLine = 0, int chipSelectLogicalPinNumber = 0, int resetPin = 0)
		{
			this.SpiBusId = spiBusId;
			this.ChipSelectLogicalPinNumber = chipSelectLogicalPinNumber;

			var settings = new SpiConnectionSettings(spiBusId, chipSelectLine)
			{
				ClockFrequency = 500000,
				Mode = SpiMode.Mode0,
			};

			this.SX127XTransceiver = SpiDevice.Create(settings);

			if ((chipSelectLogicalPinNumber != 0) || (resetPin != 0))
			{
				gpioController = new GpioController(PinNumberingScheme.Logical);
			}

			if (ChipSelectLogicalPinNumber != 0)
			{
				gpioController.OpenPin(ChipSelectLogicalPinNumber, PinMode.Output);
				gpioController.Write(ChipSelectLogicalPinNumber, PinValue.High);
			}

			if (resetPin != 0)
			{
				gpioController.OpenPin(resetPin, PinMode.Output);
				gpioController.Write(resetPin, PinValue.Low);

				Thread.Sleep(20);
				gpioController.Write(resetPin, PinValue.High);
				Thread.Sleep(20);
			}
		}

		public Byte ReadByte(byte registerAddress)
		{
			Span<byte> writeBuffer = stackalloc byte[2];
			Span<byte> readBuffer = stackalloc byte[writeBuffer.Length];

			if (SX127XTransceiver == null)
			{
				throw new ApplicationException("SX127XDevice is not initialised");
			}

			writeBuffer[0] = registerAddress &= RegisterAddressReadMask;

			if (this.ChipSelectLogicalPinNumber != 0)
			{
				gpioController.Write(ChipSelectLogicalPinNumber, PinValue.Low);
			}

			this.SX127XTransceiver.TransferFullDuplex(writeBuffer, readBuffer);

			if (this.ChipSelectLogicalPinNumber != 0)
			{
				gpioController.Write(ChipSelectLogicalPinNumber, PinValue.High);
			}

			return readBuffer[writeBuffer.Length - 1];
		}

		public ushort ReadWordMsbLsb(byte registerAddress)
		{
			Span<byte> writeBuffer = stackalloc byte[3];
			Span<byte> readBuffer = stackalloc byte[writeBuffer.Length];

			if (SX127XTransceiver == null)
			{
				throw new ApplicationException("SX127XDevice is not initialised");
			}

			writeBuffer[0] = registerAddress &= RegisterAddressReadMask;

			if (this.ChipSelectLogicalPinNumber != 0)
			{
				gpioController.Write(ChipSelectLogicalPinNumber, PinValue.Low);
			}

			this.SX127XTransceiver.TransferFullDuplex(writeBuffer, readBuffer);

			if (this.ChipSelectLogicalPinNumber != 0)
			{
				gpioController.Write(ChipSelectLogicalPinNumber, PinValue.High);
			}

			return (ushort)((readBuffer[1] << 8) + readBuffer[2]);
		}

		public byte[] ReadBytes(byte registerAddress, byte length)
		{
			Span<byte> writeBuffer = stackalloc byte[length + 1];
			Span<byte> readBuffer = stackalloc byte[writeBuffer.Length];

			if (SX127XTransceiver == null)
			{
				throw new ApplicationException("SX127XDevice is not initialised");
			}

			writeBuffer[0] = registerAddress &= RegisterAddressReadMask;

			if (this.ChipSelectLogicalPinNumber != 0)
			{
				gpioController.Write(ChipSelectLogicalPinNumber, PinValue.Low);
			}

			this.SX127XTransceiver.TransferFullDuplex(writeBuffer, readBuffer);

			if (this.ChipSelectLogicalPinNumber != 0)
			{
				gpioController.Write(ChipSelectLogicalPinNumber, PinValue.High);
			}

			return readBuffer[1..readBuffer.Length].ToArray();
		}

		public void WriteByte(byte registerAddress, byte value)
		{
			Span<byte> writeBuffer = stackalloc byte[2];
			Span<byte> readBuffer = stackalloc byte[writeBuffer.Length];

			if (SX127XTransceiver == null)
			{
				throw new ApplicationException("SX127XDevice is not initialised");
			}

			writeBuffer[0] = registerAddress |= RegisterAddressWriteMask;
			writeBuffer[1] = value;

			if (this.ChipSelectLogicalPinNumber != 0)
			{
				gpioController.Write(ChipSelectLogicalPinNumber, PinValue.Low);
			}

			this.SX127XTransceiver.TransferFullDuplex(writeBuffer, readBuffer);

			if (this.ChipSelectLogicalPinNumber != 0)
			{
				gpioController.Write(ChipSelectLogicalPinNumber, PinValue.High);
			}
		}

		public void WriteWordMsbLsb(byte address, ushort value)
		{
			Span<byte> writeBuffer = stackalloc byte[3];
			Span<byte> readBuffer = stackalloc byte[writeBuffer.Length];

			if (SX127XTransceiver == null)
			{
				throw new ApplicationException("SX127XDevice is not initialised");
			}

			writeBuffer[0] = address |= RegisterAddressWriteMask;
			writeBuffer[1] = (byte)(value >> 8);
			writeBuffer[2] = (byte)value;

			if (this.ChipSelectLogicalPinNumber != 0)
			{
				gpioController.Write(ChipSelectLogicalPinNumber, PinValue.Low);
			}

			this.SX127XTransceiver.TransferFullDuplex(writeBuffer, readBuffer);

			if (this.ChipSelectLogicalPinNumber != 0)
			{
				gpioController.Write(ChipSelectLogicalPinNumber, PinValue.High);
			}
		}

		public void WriteBytes(byte address, byte[] bytes)
		{
			Span<byte> writeBuffer = stackalloc byte[bytes.Length + 1];
			Span<byte> readBuffer = stackalloc byte[writeBuffer.Length];

			if (SX127XTransceiver == null)
			{
				throw new ApplicationException("SX127XDevice is not initialised");
			}

			writeBuffer[0] = address |= RegisterAddressWriteMask;
			for (byte index = 0; index < bytes.Length; index++)
			{
				writeBuffer[index + 1] = bytes[index];
			}

			if (this.ChipSelectLogicalPinNumber != 0)
			{
				gpioController.Write(ChipSelectLogicalPinNumber, PinValue.Low);
			}

			this.SX127XTransceiver.TransferFullDuplex(writeBuffer, readBuffer);

			if (this.ChipSelectLogicalPinNumber != 0)
			{
				gpioController.Write(ChipSelectLogicalPinNumber, PinValue.High);
			}
		}

		public void RegisterDump()
		{
			Debug.WriteLine("Register dump");

			if (SX127XTransceiver == null)
			{
				throw new ApplicationException("SX127XDevice is not initialised");
			}

			for (byte registerIndex = RegisterAddressMinimum; registerIndex <= RegisterAddressMaximum; registerIndex++)
			{
				byte registerValue = this.ReadByte(registerIndex);

				Debug.WriteLine($"Register 0x{registerIndex:x2} - Value 0X{registerValue:x2} - Bits {Convert.ToString(registerValue, 2).PadLeft(8, '0')}");
			}

			Debug.WriteLine("");
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			// Uptronics has no reset pin uses CS0 or CS1
			//SX127XDevice sX127XDevice = new SX127XDevice(chipSelectLine: 0); 
			//SX127XDevice sX127XDevice = new SX127XDevice(chipSelectLine: 1); 

			// M2M device has reset pin uses non standard chip select 
			//SX127XDevice sX127XDevice = new SX127XDevice(chipSelectLine: 0, chipSelectLogicalPinNumber: 25, resetPin: 17);
			SX127XDevice sX127XDevice = new SX127XDevice(chipSelectLine: 1, chipSelectLogicalPinNumber: 25, resetPin: 17);

			// Put device into LoRa + Sleep mode
			sX127XDevice.WriteByte(0x01, 0b10000000); // RegOpMode 

			// Set the frequency to 915MHz
			byte[] frequencyBytes = { 0xE4, 0xC0, 0x00 }; // RegFrMsb, RegFrMid, RegFrLsb
			sX127XDevice.WriteBytes(0x06, frequencyBytes);

			sX127XDevice.WriteByte(0x0F, 0x0); // RegFifoRxBaseAddress 

			sX127XDevice.WriteByte(0x01, 0b10000101); // RegOpMode set LoRa & RxContinuous

			while (true)
			{
				// Wait until a packet is received, no timeouts in PoC
				Console.WriteLine("Receive-Wait");
				byte IrqFlags = sX127XDevice.ReadByte(0x12); // RegIrqFlags
				while ((IrqFlags & 0b01000000) == 0)  // wait until RxDone cleared
				{
					Thread.Sleep(100);
					IrqFlags = sX127XDevice.ReadByte(0x12); // RegIrqFlags
					Debug.Write(".");
				}
				Console.WriteLine("");
				Console.WriteLine(string.Format("RegIrqFlags {0}", Convert.ToString((byte)IrqFlags, 2).PadLeft(8, '0')));
				Console.WriteLine("Receive-Message");
				byte currentFifoAddress = sX127XDevice.ReadByte(0x10); // RegFifiRxCurrent
				sX127XDevice.WriteByte(0x0d, currentFifoAddress); // RegFifoAddrPtr

				byte numberOfBytes = sX127XDevice.ReadByte(0x13); // RegRxNbBytes

				// Allocate buffer for message
				byte[] messageBytes = new byte[numberOfBytes];

				for (int i = 0; i < numberOfBytes; i++)
				{
					messageBytes[i] = sX127XDevice.ReadByte(0x00); // RegFifo
				}
				sX127XDevice.WriteByte(0x0d, 0);
				sX127XDevice.WriteByte(0x12, 0b11111111); // RegIrqFlags clear all the bits

				string messageText = UTF8Encoding.UTF8.GetString(messageBytes);
				Console.WriteLine("Received {0} byte message {1}", messageBytes.Length, messageText);

				Console.WriteLine("Receive-Done");
			}
		}
	}
}

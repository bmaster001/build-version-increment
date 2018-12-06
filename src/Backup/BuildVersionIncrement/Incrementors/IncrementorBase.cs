// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: BaseIncrementor.cs
// ----------------------------------------------------------------------
// Created and maintained by Paul J. Melia.
// Copyright © 2016 Paul J. Melia.
// All rights reserved.
// ----------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR 
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT 
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// ----------------------------------------------------------------------

namespace BuildVersionIncrement.Incrementors
{
	using System;
	using System.ComponentModel;

	using Model;

	[TypeConverter(typeof(IncrementorBaseConverter))]
	public abstract class IncrementorBase
	{
		protected IncrementorBase()
		{
			Initialise();
		}

		protected void Initialise()
		{
			if (string.IsNullOrEmpty(Name) || Name.Contains("."))
			{
				throw (new FormatException($"The Name property of the class {GetType().FullName} is invalid."));
			}
		}

		public abstract string Description { get; }

		public abstract string Name { get; }

		public abstract void Execute(IncrementContext context, VersionComponent versionComponent);
	}
}
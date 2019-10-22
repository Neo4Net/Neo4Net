using System;
using System.Text;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.@unsafe.Impl.Internal.Dragons
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.os.OsBeanUtil.VALUE_UNAVAILABLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.os.OsBeanUtil.getCommittedVirtualMemory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.os.OsBeanUtil.getFreePhysicalMemory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.os.OsBeanUtil.getTotalPhysicalMemory;

	public class NativeMemoryAllocationRefusedError : Exception
	{
		 private readonly long _attemptedAllocationSizeBytes;
		 private readonly long _alreadyAllocatedBytes;

		 internal NativeMemoryAllocationRefusedError( long size, long alreadyAllocatedBytes, Exception cause ) : base( cause )
		 {
			  this._attemptedAllocationSizeBytes = size;
			  this._alreadyAllocatedBytes = alreadyAllocatedBytes;
		 }

		 public override string Message
		 {
			 get
			 {
				  string message = base.Message;
				  StringBuilder sb = new StringBuilder();
				  sb.Append( "Failed to allocate " ).Append( _attemptedAllocationSizeBytes ).Append( " bytes. " );
				  sb.Append( "So far " ).Append( _alreadyAllocatedBytes );
				  sb.Append( " bytes have already been successfully allocated. " );
				  sb.Append( "The system currently has " );
				  AppendBytes( sb, TotalPhysicalMemory ).Append( " total physical memory, " );
				  AppendBytes( sb, CommittedVirtualMemory ).Append( " committed virtual memory, and " );
				  AppendBytes( sb, FreePhysicalMemory ).Append( " free physical memory. " );
				  sb.Append( "Relevant system properties: " );
				  AppendSysProp( sb, "java.vm.name" );
				  AppendSysProp( sb.Append( ", " ), "java.vm.vendor" );
				  AppendSysProp( sb.Append( ", " ), "os.arch" );
   
				  if ( Cause is System.OutOfMemoryException )
				  {
						sb.Append( ". The allocation was refused by the operating system" );
				  }
   
				  if ( !string.ReferenceEquals( message, null ) )
				  {
						sb.Append( ": " ).Append( message );
				  }
				  else
				  {
						sb.Append( '.' );
				  }
				  return sb.ToString();
			 }
		 }

		 private StringBuilder AppendBytes( StringBuilder sb, long bytes )
		 {
			  if ( bytes == VALUE_UNAVAILABLE )
			  {
					sb.Append( "(?) bytes" );
			  }
			  else
			  {
					sb.Append( bytes ).Append( " bytes" );
			  }
			  return sb;
		 }

		 private void AppendSysProp( StringBuilder sb, string sysProp )
		 {
			  sb.Append( '"' ).Append( sysProp ).Append( "\" = \"" ).Append( System.getProperty( sysProp ) ).Append( '"' );
		 }
	}

}
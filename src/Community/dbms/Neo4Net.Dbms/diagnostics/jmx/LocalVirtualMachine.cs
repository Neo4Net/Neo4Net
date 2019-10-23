using System;

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
namespace Neo4Net.Dbms.diagnostics.jmx
{
	using AttachNotSupportedException = com.sun.tools.attach.AttachNotSupportedException;
	using VirtualMachine = com.sun.tools.attach.VirtualMachine;


	public class LocalVirtualMachine
	{
		 private readonly string _address;
		 private readonly Properties _systemProperties;

		 private LocalVirtualMachine( string address, Properties systemProperties )
		 {
			  this._address = address;
			  this._systemProperties = systemProperties;
		 }

		 public virtual string JmxAddress
		 {
			 get
			 {
				  return _address;
			 }
		 }

		 public virtual Properties SystemProperties
		 {
			 get
			 {
				  return _systemProperties;
			 }
		 }

		 /// <summary>
		 /// Get an instance from a process id and makes sure the a JMX agent is running on it.
		 /// </summary>
		 /// <param name="pid"> process id of the jvm to attach to. </param>
		 /// <returns> a virtual machine with a JMX endpoint available. </returns>
		 /// <exception cref="IOException"> if any operations failed. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static LocalVirtualMachine from(long pid) throws java.io.IOException
		 public static LocalVirtualMachine From( long pid )
		 {
			  VirtualMachine vm = null;
			  try
			  {
					// Try to attach to instance
					vm = VirtualMachine.attach( pid.ToString() );

					// Get local jmx address if management agent is already started
					Properties agentProps = vm.AgentProperties;
					string address = ( string ) agentProps.get( "com.sun.management.jmxremote.localConnectorAddress" );

					// Failed, we are the first one connecting, start agent
					if ( string.ReferenceEquals( address, null ) )
					{
						 address = vm.startLocalManagementAgent();
					}

					return new LocalVirtualMachine( address, vm.SystemProperties );
			  }
			  catch ( AttachNotSupportedException x )
			  {
					throw new IOException( x.Message, x );
			  }
			  catch ( Exception e )
			  {
					// ibm jdk uses a separate exception
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
					if ( e.GetType().FullName.Equals("com.ibm.tools.attach.AttachNotSupportedException") )
					{
						 throw new IOException( e );
					}
					throw e;
			  }
			  finally
			  {
					if ( vm != null )
					{
						 vm.detach();
					}
			  }
		 }
	}

}
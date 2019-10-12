using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Ports.Allocation
{

	/// <summary>
	/// A source for free ports on this machine
	/// </summary>
	public class PortAuthority
	{
		 // this is quite an arbitrary choice and not currently configurable - but it works.
		 private const int PORT_RANGE_MINIMUM = 20000;

		 private static readonly PortProvider _portProvider;

		 static PortAuthority()
		 {
			  string portAuthorityDirectory = System.getProperty( "port.authority.directory" );

			  if ( string.ReferenceEquals( portAuthorityDirectory, null ) )
			  {
					_portProvider = new SimplePortProvider( new DefaultPortProbe(), PORT_RANGE_MINIMUM );
			  }
			  else
			  {
					try
					{
						 Path directory = Paths.get( portAuthorityDirectory );
						 Files.createDirectories( directory );
						 PortRepository portRepository = new PortRepository( directory, PORT_RANGE_MINIMUM );
						 PortProbe portProbe = new DefaultPortProbe();
						 _portProvider = new CoordinatingPortProvider( portRepository, portProbe );
					}
					catch ( IOException e )
					{
						 throw new ExceptionInInitializerError( e );
					}
			  }
		 }

		 public static int AllocatePort()
		 {
			  string trace = BuildTrace();

			  return _portProvider.getNextFreePort( trace );
		 }

		 private static string BuildTrace()
		 {
			  MemoryStream outputStream = new MemoryStream();

			  using ( PrintWriter printWriter = new PrintWriter( outputStream ) )
			  {
					( new Exception() ).printStackTrace(printWriter);
			  }

			  return StringHelper.NewString( outputStream.toByteArray() );
		 }
	}

}
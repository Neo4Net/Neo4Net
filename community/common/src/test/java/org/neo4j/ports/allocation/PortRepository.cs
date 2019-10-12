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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ports.allocation.PortConstants.EphemeralPortMaximum;

	internal class PortRepository
	{
		 private readonly Path _directory;

		 private int _currentPort;

		 internal PortRepository( Path directory, int initialPort )
		 {
			  this._directory = directory;

			  this._currentPort = initialPort;
		 }

		 // synchronize between threads in this JVM
		 internal virtual int ReserveNextPort( string trace )
		 {
			 lock ( this )
			 {
				  while ( _currentPort <= EphemeralPortMaximum )
				  {
						Path portFilePath = _directory.resolve( "port" + _currentPort );
      
						try
						{
							 // synchronize between processes on this machine
							 Files.createFile( portFilePath );
      
							 // write a trace for debugging purposes
							 using ( FileStream fileOutputStream = new FileStream( portFilePath.toFile(), true ) )
							 {
								  fileOutputStream.WriteByte( trace.GetBytes() );
								  fileOutputStream.Flush();
							 }
      
							 return _currentPort++;
						}
						catch ( FileAlreadyExistsException )
						{
							 _currentPort++;
						}
						catch ( IOException e )
						{
							 throw new System.InvalidOperationException( "This will never happen - LWN", e );
						}
				  }
      
				  throw new System.InvalidOperationException( "There are no more ports available" );
			 }
		 }
	}

}
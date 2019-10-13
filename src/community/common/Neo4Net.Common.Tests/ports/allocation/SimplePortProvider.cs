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
namespace Neo4Net.Ports.Allocation
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ports.allocation.PortConstants.EphemeralPortMaximum;

	/// <summary>
	/// Port provider that relies on state in a single JVM. Not suitable for parallel test execution (as in, several JVM
	/// processes executing tests). _Is_ suitable for multi-threaded execution.
	/// </summary>
	public class SimplePortProvider : PortProvider
	{
		 private readonly PortProbe _portProbe;

		 private int _currentPort;

		 public SimplePortProvider( PortProbe portProbe, int initialPort )
		 {
			  this._portProbe = portProbe;

			  this._currentPort = initialPort;
		 }

		 public override int GetNextFreePort( string ignored )
		 {
			 lock ( this )
			 {
				  while ( _currentPort <= EphemeralPortMaximum )
				  {
						if ( !_portProbe.isOccupied( _currentPort ) )
						{
							 return _currentPort++;
						}
      
						_currentPort++;
				  }
      
				  throw new System.InvalidOperationException( "There are no more ports available" );
			 }
		 }
	}

}
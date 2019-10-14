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
namespace Neo4Net.Logging
{
	/// <summary>
	/// A <seealso cref="LogProvider"/> implementation that discards all messages
	/// </summary>
	public class NullLogProvider : LogProvider
	{
		 private static readonly NullLogProvider _instance = new NullLogProvider();

		 private NullLogProvider()
		 {
		 }

		 /// <returns> A singleton <seealso cref="NullLogProvider"/> instance </returns>
		 public static NullLogProvider Instance
		 {
			 get
			 {
				  return _instance;
			 }
		 }

		 public override Log GetLog( Type loggingClass )
		 {
			  return NullLog.Instance;
		 }

		 public override Log GetLog( string name )
		 {
			  return NullLog.Instance;
		 }
	}

}
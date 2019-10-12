/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.com
{
	/// <summary>
	/// Thrown when a communication between client/server is attempted and either of internal protocol version
	/// and application protocol doesn't match.
	/// 
	/// @author Mattias Persson
	/// </summary>
	public class IllegalProtocolVersionException : ComException
	{
		 private readonly sbyte _expected;
		 private readonly sbyte _received;

		 public IllegalProtocolVersionException( sbyte expected, sbyte received, string message ) : base( message )
		 {
			  this._expected = expected;
			  this._received = received;
		 }

		 public virtual sbyte Expected
		 {
			 get
			 {
				  return _expected;
			 }
		 }

		 public virtual sbyte Received
		 {
			 get
			 {
				  return _received;
			 }
		 }

		 public override string ToString()
		 {
			  return "IllegalProtocolVersionException{expected=" + _expected + ", received=" + _received + "}";
		 }
	}

}
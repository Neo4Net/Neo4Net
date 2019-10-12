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
namespace Neo4Net.Bolt.testing
{
	using BoltResponseHandler = Neo4Net.Bolt.runtime.BoltResponseHandler;
	using BoltResult = Neo4Net.Bolt.runtime.BoltResult;
	using Neo4jError = Neo4Net.Bolt.runtime.Neo4jError;
	using AnyValue = Neo4Net.Values.AnyValue;

	/// <summary>
	/// Used by tests when the response for a request is not relevant.
	/// </summary>
	public class NullResponseHandler : BoltResponseHandler
	{
		 private static readonly NullResponseHandler _instance = new NullResponseHandler();

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static NullResponseHandler NullResponseHandlerConflict()
		 {
			  return _instance;
		 }

		 private NullResponseHandler()
		 {
		 }

		 public override void OnRecords( BoltResult result, bool pull )
		 {
			  // this page intentionally left blank
		 }

		 public override void OnMetadata( string key, AnyValue value )
		 {
			  // this page intentionally left blank
		 }

		 public override void MarkFailed( Neo4jError error )
		 {
			  // this page intentionally left blank
		 }

		 public override void OnFinish()
		 {
			  // this page intentionally left blank
		 }

		 public override void MarkIgnored()
		 {
			  // this page intentionally left blank
		 }

	}

}
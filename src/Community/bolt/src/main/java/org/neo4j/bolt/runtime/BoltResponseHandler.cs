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
namespace Neo4Net.Bolt.runtime
{
	using AnyValue = Neo4Net.Values.AnyValue;

	/// <summary>
	/// Callback for handling the result of requests. For a given session, callbacks will be invoked serially,
	/// in the order they were given. This means you may pass the same callback multiple times without waiting for a
	/// reply, and are guaranteed that your callbacks will be called in order.
	/// </summary>
	public interface BoltResponseHandler
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void onRecords(BoltResult result, boolean pull) throws Exception;
		 void OnRecords( BoltResult result, bool pull );

		 void OnMetadata( string key, AnyValue value );

		 /// <summary>
		 /// Called when the state machine ignores an operation, because it is waiting for an error to be acknowledged </summary>
		 void MarkIgnored();

		 /// <summary>
		 /// Called zero or more times if there are failures </summary>
		 void MarkFailed( Neo4jError error );

		 /// <summary>
		 /// Called when the operation is completed. </summary>
		 void OnFinish();
	}

}
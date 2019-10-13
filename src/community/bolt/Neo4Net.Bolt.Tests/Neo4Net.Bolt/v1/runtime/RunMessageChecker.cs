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
namespace Neo4Net.Bolt.v1.runtime
{

	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;

	internal sealed class RunMessageChecker
	{
		 private static readonly Pattern _begin = Pattern.compile( "(?i)^\\s*BEGIN\\s*;?\\s*$" );
		 private static readonly Pattern _commit = Pattern.compile( "(?i)^\\s*COMMIT\\s*;?\\s*$" );
		 private static readonly Pattern _rollback = Pattern.compile( "(?i)^\\s*ROLLBACK\\s*;?\\s*$" );

		 private RunMessageChecker()
		 {
		 }

		 internal static bool IsBegin( RunMessage message )
		 {
			  return Matches( message, _begin );
		 }

		 internal static bool IsCommit( RunMessage message )
		 {
			  return Matches( message, _commit );
		 }

		 internal static bool IsRollback( RunMessage message )
		 {
			  return Matches( message, _rollback );
		 }

		 private static bool Matches( RunMessage message, Pattern pattern )
		 {
			  return pattern.matcher( message.Statement() ).matches();
		 }
	}

}
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
namespace Neo4Net.Graphdb
{
	/// <summary>
	/// Representation for notifications found when executing a query.
	/// 
	/// A notification can be visualized in a client pinpointing problems or other information about the query.
	/// </summary>
	public interface Notification
	{
		 /// <summary>
		 /// Returns a notification code for the discovered issue. </summary>
		 /// <returns> the notification code </returns>
		 string Code { get; }

		 /// <summary>
		 /// Returns a short summary of the notification. </summary>
		 /// <returns> the title of the notification. </returns>
		 string Title { get; }

		 /// <summary>
		 /// Returns a longer description of the notification. </summary>
		 /// <returns> the description of the notification. </returns>
		 string Description { get; }

		 /// <summary>
		 /// Returns the severity level of this notification. </summary>
		 /// <returns> the severity level of the notification. </returns>
		 SeverityLevel Severity { get; }

		 /// <summary>
		 /// The position in the query where this notification points to.
		 /// Not all notifications have a unique position to point to and should in
		 /// that case return <seealso cref="org.neo4j.graphdb.InputPosition.empty"/>
		 /// </summary>
		 /// <returns> the position in the query where the issue was found, or
		 /// <seealso cref="org.neo4j.graphdb.InputPosition.empty"/> if no position is associated with this notification. </returns>
		 InputPosition Position { get; }
	}

}
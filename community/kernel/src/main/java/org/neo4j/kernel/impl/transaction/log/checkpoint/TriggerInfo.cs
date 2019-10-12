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
namespace Org.Neo4j.Kernel.impl.transaction.log.checkpoint
{

	/// <summary>
	/// A {@code TriggerInfo} contains the information about the events that are triggering a check point.
	/// 
	/// The <seealso cref="Consumer<string>.accept(string)"/> method can be used to enrich the description with
	/// extra information. As an example, when the events triggering the check point are conditionalized wrt to a threshold,
	/// thi can be used for adding the information about the threshold that actually allowed the check point to happen.
	/// </summary>
	public interface TriggerInfo : System.Action<string>
	{
		 /// <summary>
		 /// This method can be used to retrieve the actual human-readable description about the events that triggered the
		 /// check point.
		 /// </summary>
		 /// <param name="transactionId"> the transaction id we are check pointing on </param>
		 /// <returns> the description of the events that triggered check pointing </returns>
		 string Describe( long transactionId );
	}

}
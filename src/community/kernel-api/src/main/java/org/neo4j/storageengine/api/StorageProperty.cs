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
namespace Neo4Net.Storageengine.Api
{
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// Abstraction pairing property key token id and property value. Can represent both defined and undefined
	/// property values, distinguished by <seealso cref="isDefined()"/>. Undefined property instances can be used for
	/// passing property key id and signaling that it doesn't exist.
	/// </summary>
	public interface StorageProperty
	{
		 /// <returns> property key token id for this property. </returns>
		 int PropertyKeyId();

		 /// <returns> the property value or Values.NO_VALUE is this property does not exist. </returns>
		 Value Value();

		 /// <returns> whether or not the property is defined, e.g. if it exists (has a value) or not. </returns>
		 bool Defined { get; }
	}

}
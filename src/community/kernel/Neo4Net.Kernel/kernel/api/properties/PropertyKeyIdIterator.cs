using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Api.properties
{
	using IntIterator = org.eclipse.collections.api.iterator.IntIterator;

	using StorageProperty = Neo4Net.Kernel.Api.StorageEngine.StorageProperty;

	public class PropertyKeyIdIterator : IntIterator
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Iterator<? extends Neo4Net.Kernel.Api.StorageEngine.StorageProperty> properties;
		 private readonly IEnumerator<StorageProperty> _properties;

		 public PropertyKeyIdIterator<T1>( IEnumerator<T1> properties ) where T1 : Neo4Net.Kernel.Api.StorageEngine.StorageProperty
		 {
			  this._properties = properties;
		 }

		 public override bool HasNext()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return _properties.hasNext();
		 }

		 public override int Next()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return _properties.next().propertyKeyId();
		 }
	}

}
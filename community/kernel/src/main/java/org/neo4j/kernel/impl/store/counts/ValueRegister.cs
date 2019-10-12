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
namespace Org.Neo4j.Kernel.impl.store.counts
{
	using Org.Neo4j.Kernel.impl.store.kvstore;
	using ReadableBuffer = Org.Neo4j.Kernel.impl.store.kvstore.ReadableBuffer;
	using Register = Org.Neo4j.Register.Register;

	internal class ValueRegister : AbstractKeyValueStore.Reader<Org.Neo4j.Register.Register_DoubleLongRegister>
	{
		 private readonly Org.Neo4j.Register.Register_DoubleLongRegister _target;

		 internal ValueRegister( Org.Neo4j.Register.Register_DoubleLongRegister target )
		 {
			  this._target = target;
		 }

		 protected internal override Org.Neo4j.Register.Register_DoubleLongRegister ParseValue( ReadableBuffer value )
		 {
			  _target.write( value.GetLong( 0 ), value.GetLong( 8 ) );
			  return _target;
		 }

		 protected internal override Org.Neo4j.Register.Register_DoubleLongRegister DefaultValue()
		 {
			  _target.write( 0, 0 );
			  return _target;
		 }
	}

}
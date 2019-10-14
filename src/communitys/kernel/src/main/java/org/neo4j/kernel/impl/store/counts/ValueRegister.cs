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
namespace Neo4Net.Kernel.impl.store.counts
{
	using Neo4Net.Kernel.impl.store.kvstore;
	using ReadableBuffer = Neo4Net.Kernel.impl.store.kvstore.ReadableBuffer;
	using Register = Neo4Net.Register.Register;

	internal class ValueRegister : AbstractKeyValueStore.Reader<Neo4Net.Register.Register_DoubleLongRegister>
	{
		 private readonly Neo4Net.Register.Register_DoubleLongRegister _target;

		 internal ValueRegister( Neo4Net.Register.Register_DoubleLongRegister target )
		 {
			  this._target = target;
		 }

		 protected internal override Neo4Net.Register.Register_DoubleLongRegister ParseValue( ReadableBuffer value )
		 {
			  _target.write( value.GetLong( 0 ), value.GetLong( 8 ) );
			  return _target;
		 }

		 protected internal override Neo4Net.Register.Register_DoubleLongRegister DefaultValue()
		 {
			  _target.write( 0, 0 );
			  return _target;
		 }
	}

}
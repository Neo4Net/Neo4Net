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
namespace Neo4Net.Kernel.impl.store.id
{

	using KernelTransactionsSnapshot = Neo4Net.Kernel.Impl.Api.KernelTransactionsSnapshot;

	internal class BufferingIdGenerator : IdGenerator_Delegate
	{
		 private DelayedBuffer<KernelTransactionsSnapshot> _buffer;

		 internal BufferingIdGenerator( IdGenerator @delegate ) : base( @delegate )
		 {
		 }

		 internal virtual void Initialize( System.Func<KernelTransactionsSnapshot> boundaries, System.Predicate<KernelTransactionsSnapshot> safeThreshold )
		 {
			  _buffer = new DelayedBuffer<KernelTransactionsSnapshot>(boundaries, safeThreshold, 10_000, freedIds =>
			  {
				foreach ( long id in freedIds )
				{
					 ActualFreeId( id );
				}
			  });
		 }

		 private void ActualFreeId( long id )
		 {
			  base.FreeId( id );
		 }

		 public override void FreeId( long id )
		 {
			  _buffer.offer( id );
		 }

		 internal virtual void Maintenance()
		 {
			  _buffer.maintenance();
		 }

		 internal virtual void Clear()
		 {
			  _buffer.clear();
		 }

		 public override void Close()
		 {
			  if ( _buffer != null )
			  {
					_buffer.close();
			  }
			  base.Dispose();
		 }
	}

}
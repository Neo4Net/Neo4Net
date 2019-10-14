using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.tools.txlog.checktypes
{

	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using PropertyCommand = Neo4Net.Kernel.impl.transaction.command.Command.PropertyCommand;

	/// <summary>
	/// Type of command (<seealso cref="NodeCommand"/>, <seealso cref="PropertyCommand"/>, ...) to check during transaction log verification.
	/// This class exists to mitigate the absence of interfaces for commands with before and after state.
	/// It also provides an alternative equality check instead of <seealso cref="AbstractBaseRecord.equals(object)"/> that only
	/// checks <seealso cref="AbstractBaseRecord.getId() entity id"/>.
	/// </summary>
	/// @param <C> the type of command to check </param>
	/// @param <R> the type of records that this command contains </param>
	public abstract class CheckType<C, R> where C : Neo4Net.Kernel.impl.transaction.command.Command where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
	{
		 private readonly Type<C> _recordClass;

		 internal CheckType( Type recordClass )
		 {
				 recordClass = typeof( C );
			  this._recordClass = recordClass;
		 }

		 public virtual Type<C> CommandClass()
		 {
			  return _recordClass;
		 }

		 public abstract R Before( C command );

		 public abstract R After( C command );

		 public bool Equal( R record1, R record2 )
		 {
			  Objects.requireNonNull( record1 );
			  Objects.requireNonNull( record2 );

			  if ( record1.Id != record2.Id )
			  {
					return false;
			  }
			  else if ( record1.inUse() != record2.inUse() )
			  {
					return false;
			  }
			  else if ( !record1.inUse() )
			  {
					return true;
			  }
			  else
			  {
					return InUseRecordsEqual( record1, record2 );
			  }
		 }

		 protected internal abstract bool InUseRecordsEqual( R record1, R record2 );

		 public abstract string Name();
	}

}
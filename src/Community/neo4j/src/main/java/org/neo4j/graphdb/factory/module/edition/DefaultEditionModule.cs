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
namespace Neo4Net.Graphdb.factory.module.edition
{

	using DatabaseEditionContext = Neo4Net.Graphdb.factory.module.edition.context.DatabaseEditionContext;
	using DefaultEditionModuleDatabaseContext = Neo4Net.Graphdb.factory.module.edition.context.DefaultEditionModuleDatabaseContext;
	using IdContextFactory = Neo4Net.Graphdb.factory.module.id.IdContextFactory;
	using CommitProcessFactory = Neo4Net.Kernel.Impl.Api.CommitProcessFactory;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using StatementLocksFactory = Neo4Net.Kernel.impl.locking.StatementLocksFactory;

	public abstract class DefaultEditionModule : AbstractEditionModule
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal CommitProcessFactory CommitProcessFactoryConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal IdContextFactory IdContextFactoryConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal System.Func<string, TokenHolders> TokenHoldersProviderConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal System.Func<Locks> LocksSupplierConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal System.Func<Locks, StatementLocksFactory> StatementLocksFactoryProviderConflict;

		 public override DatabaseEditionContext CreateDatabaseContext( string databaseName )
		 {
			  return new DefaultEditionModuleDatabaseContext( this, databaseName );
		 }

		 public virtual CommitProcessFactory CommitProcessFactory
		 {
			 get
			 {
				  return CommitProcessFactoryConflict;
			 }
		 }

		 public virtual IdContextFactory IdContextFactory
		 {
			 get
			 {
				  return IdContextFactoryConflict;
			 }
		 }

		 public virtual System.Func<string, TokenHolders> TokenHoldersProvider
		 {
			 get
			 {
				  return TokenHoldersProviderConflict;
			 }
		 }

		 public virtual System.Func<Locks> LocksSupplier
		 {
			 get
			 {
				  return LocksSupplierConflict;
			 }
		 }

		 public virtual System.Func<Locks, StatementLocksFactory> StatementLocksFactoryProvider
		 {
			 get
			 {
				  return StatementLocksFactoryProviderConflict;
			 }
		 }
	}

}
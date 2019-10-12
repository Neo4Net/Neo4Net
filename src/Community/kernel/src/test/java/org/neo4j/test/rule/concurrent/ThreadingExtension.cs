using System;

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
namespace Neo4Net.Test.rule.concurrent
{
	using AfterAllCallback = org.junit.jupiter.api.extension.AfterAllCallback;
	using AfterEachCallback = org.junit.jupiter.api.extension.AfterEachCallback;
	using BeforeEachCallback = org.junit.jupiter.api.extension.BeforeEachCallback;
	using ExtensionContext = org.junit.jupiter.api.extension.ExtensionContext;

	using Neo4Net.Test.extension;

	public class ThreadingExtension : StatefullFieldExtension<ThreadingRule>, BeforeEachCallback, AfterEachCallback, AfterAllCallback
	{
		 private const string THREADING = "threading";
		 private static readonly ExtensionContext.Namespace _threadingNamespace = ExtensionContext.Namespace.create( THREADING );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void afterEach(org.junit.jupiter.api.extension.ExtensionContext extensionContext) throws Exception
		 public override void AfterEach( ExtensionContext extensionContext )
		 {
			  ThreadingRule threadingRule = GetStoredValue( extensionContext );
			  threadingRule.After();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void beforeEach(org.junit.jupiter.api.extension.ExtensionContext extensionContext) throws Exception
		 public override void BeforeEach( ExtensionContext extensionContext )
		 {
			  ThreadingRule threadingRule = GetStoredValue( extensionContext );
			  threadingRule.Before();
		 }

		 protected internal override string FieldKey
		 {
			 get
			 {
				  return THREADING;
			 }
		 }

		 protected internal override Type<ThreadingRule> FieldType
		 {
			 get
			 {
				  return typeof( ThreadingRule );
			 }
		 }

		 protected internal override ThreadingRule CreateField( ExtensionContext extensionContext )
		 {
			  ThreadingRule threadingRule = new ThreadingRule();
			  return threadingRule;
		 }

		 protected internal override ExtensionContext.Namespace NameSpace
		 {
			 get
			 {
				  return _threadingNamespace;
			 }
		 }
	}

}
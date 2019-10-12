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
namespace Org.Neo4j.Test.extension
{
	using AfterEachCallback = org.junit.jupiter.api.extension.AfterEachCallback;
	using BeforeEachCallback = org.junit.jupiter.api.extension.BeforeEachCallback;
	using ExtensionContext = org.junit.jupiter.api.extension.ExtensionContext;
	using Namespace = org.junit.jupiter.api.extension.ExtensionContext.Namespace;

	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;

	public class SuppressOutputExtension : StatefullFieldExtension<SuppressOutput>, BeforeEachCallback, AfterEachCallback
	{
		 private const string SUPPRESS_OUTPUT = "suppressOutput";
		 private static readonly ExtensionContext.Namespace _suppressOutputNamespace = ExtensionContext.Namespace.create( SUPPRESS_OUTPUT );

		 protected internal override string FieldKey
		 {
			 get
			 {
				  return SUPPRESS_OUTPUT;
			 }
		 }

		 protected internal override Type<SuppressOutput> FieldType
		 {
			 get
			 {
				  return typeof( SuppressOutput );
			 }
		 }

		 protected internal override SuppressOutput CreateField( ExtensionContext extensionContext )
		 {
			  return SuppressOutput.suppressAll();
		 }

		 protected internal override ExtensionContext.Namespace NameSpace
		 {
			 get
			 {
				  return _suppressOutputNamespace;
			 }
		 }

		 public override void AfterEach( ExtensionContext context )
		 {
			  GetStoredValue( context ).releaseVoices( context.ExecutionException.Present );
			  RemoveStoredValue( context );
		 }

		 public override void BeforeEach( ExtensionContext context )
		 {
			  GetStoredValue( context ).captureVoices();
		 }
	}

}
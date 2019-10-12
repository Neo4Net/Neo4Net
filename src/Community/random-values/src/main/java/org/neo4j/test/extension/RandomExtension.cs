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
namespace Neo4Net.Test.extension
{
	using AfterEachCallback = org.junit.jupiter.api.extension.AfterEachCallback;
	using BeforeEachCallback = org.junit.jupiter.api.extension.BeforeEachCallback;
	using ExtensionContext = org.junit.jupiter.api.extension.ExtensionContext;
	using Namespace = org.junit.jupiter.api.extension.ExtensionContext.Namespace;
	using TestExecutionExceptionHandler = org.junit.jupiter.api.extension.TestExecutionExceptionHandler;
	using AssertionFailedError = org.opentest4j.AssertionFailedError;
	using TestAbortedException = org.opentest4j.TestAbortedException;

	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using Seed = Neo4Net.Test.rule.RandomRule.Seed;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.platform.commons.support.AnnotationSupport.findAnnotation;

	public class RandomExtension : StatefullFieldExtension<RandomRule>, BeforeEachCallback, AfterEachCallback, TestExecutionExceptionHandler
	{
		 private const string RANDOM = "random";
		 private static readonly ExtensionContext.Namespace _randomNamespace = ExtensionContext.Namespace.create( RANDOM );

		 protected internal override string FieldKey
		 {
			 get
			 {
				  return RANDOM;
			 }
		 }

		 protected internal override Type<RandomRule> FieldType
		 {
			 get
			 {
				  return typeof( RandomRule );
			 }
		 }

		 protected internal override ExtensionContext.Namespace NameSpace
		 {
			 get
			 {
				  return _randomNamespace;
			 }
		 }

		 protected internal override RandomRule CreateField( ExtensionContext extensionContext )
		 {
			  RandomRule randomRule = new RandomRule();
			  randomRule.Seed = DateTimeHelper.CurrentUnixTimeMillis();
			  return randomRule;
		 }

		 public override void BeforeEach( ExtensionContext extensionContext )
		 {
			  Optional<RandomRule.Seed> optionalSeed = findAnnotation( extensionContext.Element, typeof( RandomRule.Seed ) );
			  optionalSeed.map( RandomRule.Seed.value ).ifPresent( seed => GetStoredValue( extensionContext ).setSeed( seed ) );
		 }

		 public override void AfterEach( ExtensionContext context )
		 {
			  RemoveStoredValue( context );
		 }

		 public override void HandleTestExecutionException( ExtensionContext context, Exception t )
		 {
			  if ( t is TestAbortedException )
			  {
					return;
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long seed = getStoredValue(context).seed();
			  long seed = GetStoredValue( context ).seed();

			  // The reason we throw a new exception wrapping the actual exception here, instead of simply enhancing the message is:
			  // - AssertionFailedError has its own 'message' field, in addition to Throwable's 'detailedMessage' field
			  // - Even if 'message' field is updated the test doesn't seem to print the updated message on assertion failure
			  throw new AssertionFailedError( format( "%s [ random seed used: %dL ]", t.Message, seed ), t );
		 }
	}

}
#region -- License Terms --
//
// MessagePack for CLI
//
// Copyright (C) 2010-2015 FUJIWARA, Yusuke
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
#endregion -- License Terms --

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using MsgPack.Serialization.AbstractSerializers;
using MsgPack.Serialization.DefaultSerializers;

namespace MsgPack.Serialization.EmittingSerializers
{
	/// <summary>
	///		An <see cref="ISerializerCodeGenerationContext"/> for <see cref="AssemblyBuilderSerializerBuilder{TObject}"/>.
	/// </summary>
	internal class AssemblyBuilderCodeGenerationContext : ISerializerCodeGenerationContext
	{
		private readonly SerializationContext _context;
		private readonly SerializationMethodGeneratorManager _generatorManager;
		private readonly AssemblyBuilder _assemblyBuilder;
		private readonly bool _preferReflectionBasedSerializer;

		public AssemblyBuilderCodeGenerationContext( SerializationContext context, AssemblyBuilder assemblyBuilder, bool preferReflectionBasedSerializer )
		{
			this._context = context;
			this._assemblyBuilder = assemblyBuilder;

			DefaultSerializationMethodGeneratorManager.SetUpAssemblyBuilderAttributes( assemblyBuilder, false );
			this._generatorManager = SerializationMethodGeneratorManager.Get( assemblyBuilder );
			this._preferReflectionBasedSerializer = preferReflectionBasedSerializer;
		}

		/// <summary>
		///		Create new <see cref="AssemblyBuilderEmittingContext"/> for specified <see cref="Type"/>.
		/// </summary>
		/// <param name="type">The type will be emitted.</param>
		/// <param name="serializerBaseClass">The base class of the serializer.</param>
		/// <returns><see cref="AssemblyBuilderEmittingContext"/>.</returns>
		public AssemblyBuilderEmittingContext CreateEmittingContext( Type type, Type serializerBaseClass )
		{
			return
				new AssemblyBuilderEmittingContext(
					this._context,
					type,
					() => this._generatorManager.CreateEmitter( type, serializerBaseClass, EmitterFlavor.FieldBased ),
					() => this._generatorManager.CreateEnumEmitter( this._context, type, EmitterFlavor.FieldBased ) 
				);
		}

		/// <summary>
		///		Determines that whether built-in serializer for specified type exists or not.
		/// </summary>
		/// <param name="type">The type for check.</param>
		/// <param name="traits">The known <see cref="CollectionTraits"/> of the <paramref name="type"/>.</param>
		/// <returns>
		///   <c>true</c> if built-in serializer for specified type exists; <c>false</c>, otherwise.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public bool BuiltInSerializerExists( Type type, CollectionTraits traits )
		{
			if ( type == null )
			{
				throw new ArgumentNullException( "type" );
			}

			return GenericSerializer.IsSupported( type, traits, this._preferReflectionBasedSerializer  ) || SerializerRepository.InternalDefault.Contains( type );
		}

		/// <summary>
		///		Generates codes for this context.
		/// </summary>
		/// <returns>The path of generated files.</returns>
		public IEnumerable<string> Generate()
		{
			var assemblyFileName = this._assemblyBuilder.GetName().Name + ".dll";
			this._assemblyBuilder.Save( assemblyFileName );
			return new[] { assemblyFileName };
		}
	}
}